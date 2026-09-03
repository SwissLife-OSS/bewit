using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bewit.Compatibility.V8;
using Bewit.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Tests;

public sealed class V9EndToEndTests
{
    private const string Key = "a-production-quality-secret-with-at-least-32-bytes";

    [Fact]
    public async Task SelfContainedToken_RoundTrips_WithVersionedUrlSafeFormat()
    {
        await using ServiceProvider provider = Build(builder =>
            builder.AddToken<Payload>("download"));

        IBewitTokenGenerator<Payload> generator = provider.GetRequiredService<IBewitTokenGenerator<Payload>>();
        BewitToken<Payload> token = await generator.GenerateAsync(new Payload("document-1"));
        Payload payload = await provider.GetRequiredService<IBewitTokenValidator<Payload>>()
            .ValidateAsync(token);

        token.ToString().Should().StartWith("bwt1.");
        token.ToString().Should().NotContain("+").And.NotContain("/").And.NotContain("=");
        payload.Should().Be(new Payload("document-1"));
    }

    [Fact]
    public async Task V9Token_DoesNotDowngrade_WhenSignatureIsInvalid()
    {
        await using ServiceProvider provider = Build(builder =>
            builder.AddToken<Payload>("download", token => token.AcceptV8Tokens(v8 =>
            {
                v8.Secret = "legacy-secret";
                v8.PayloadTypeName = typeof(Payload).FullName;
                v8.AcceptUntil = DateTimeOffset.MaxValue;
            })));
        string value = (await provider.GetRequiredService<IBewitTokenGenerator<Payload>>()
            .GenerateAsync(new Payload("document-1"))).ToString();
        string tampered = value[..^1] + (value[^1] == 'A' ? 'B' : 'A');

        Func<Task> action = async () => await provider
            .GetRequiredService<IBewitTokenValidator<Payload>>()
            .ValidateAsync(new BewitToken<Payload>(tampered));

        await action.Should().ThrowAsync<BewitInvalidException>();
    }

    [Fact]
    public async Task Purpose_IsSignedAndEnforced()
    {
        await using ServiceProvider issuer = Build(builder => builder.AddToken<Payload>("download"));
        BewitToken<Payload> token = await issuer.GetRequiredService<IBewitTokenGenerator<Payload>>()
            .GenerateAsync(new Payload("document-1"));
        await using ServiceProvider verifier = Build(builder => builder.AddToken<Payload>("preview"));

        Func<Task> action = async () => await verifier
            .GetRequiredService<IBewitTokenValidator<Payload>>().ValidateAsync(token);

        await action.Should().ThrowAsync<BewitInvalidException>();
    }

    [Fact]
    public async Task PreviousSigningKey_RemainsValidDuringRotation()
    {
        await using ServiceProvider issuer = Build(builder => builder.AddToken<Payload>("download"));
        BewitToken<Payload> token = await issuer.GetRequiredService<IBewitTokenGenerator<Payload>>()
            .GenerateAsync(new Payload("document-1"));
        await using ServiceProvider verifier = Build(
            builder => builder.AddToken<Payload>("download"),
            options =>
            {
                options.CurrentKeyId = "next";
                options.SigningKeys["current"] = Key;
                options.SigningKeys["next"] = "another-production-quality-secret-with-32-bytes";
            });

        Payload payload = await verifier.GetRequiredService<IBewitTokenValidator<Payload>>()
            .ValidateAsync(token);

        payload.Value.Should().Be("document-1");
    }

    [Fact]
    public async Task ServerControlledToken_CanBeExtendedAfterItExpired()
    {
        var clock = new TestTimeProvider(DateTimeOffset.Parse("2026-09-02T10:00:00Z"));
        await using ServiceProvider provider = Build(builder =>
        {
            builder.AddStateStore<V9MemoryStore>();
            builder.AddToken<Payload>("share", token => token.UseServerControlledExpiration());
        }, timeProvider: clock);
        IBewitTokenGenerator<Payload> generator = provider.GetRequiredService<IBewitTokenGenerator<Payload>>();
        BewitToken<Payload> token = await generator.GenerateAsync(
            new Payload("share-1"),
            new BewitTokenOptions { Lifetime = TimeSpan.FromMinutes(1), Identifier = "share-1" });
        clock.Advance(TimeSpan.FromMinutes(2));

        Func<Task> expired = async () => await provider
            .GetRequiredService<IBewitTokenValidator<Payload>>().ValidateAsync(token);
        await expired.Should().ThrowAsync<BewitExpiredException>();

        BewitBulkOperationResult result = await provider
            .GetRequiredService<IBewitTokenRepository<Payload>>()
            .UpdateExpirationByIdentifierAsync("share-1", clock.GetUtcNow().AddMinutes(5), default);
        Payload payload = await provider.GetRequiredService<IBewitTokenValidator<Payload>>()
            .ValidateAsync(token);

        result.GetAffectedCount(BewitTokenFormat.V9).Should().Be(1);
        payload.Value.Should().Be("share-1");
    }

    [Fact]
    public async Task SingleUseToken_IsConsumedAtomically()
    {
        await using ServiceProvider provider = Build(builder =>
        {
            builder.AddStateStore<V9MemoryStore>();
            builder.AddToken<Payload>("share", token => token
                .UseServerControlledExpiration()
                .UseSingleUseTokens());
        });
        BewitToken<Payload> token = await provider.GetRequiredService<IBewitTokenGenerator<Payload>>()
            .GenerateAsync(new Payload("share-1"));
        IBewitTokenValidator<Payload> validator = provider.GetRequiredService<IBewitTokenValidator<Payload>>();

        await validator.ValidateAsync(token);
        Func<Task> second = async () => await validator.ValidateAsync(token);

        await second.Should().ThrowAsync<BewitAlreadyConsumedException>();
    }

    [Fact]
    public async Task LegacyPolicy_CanUpdateLoadedRecordAndRequestOneRefresh()
    {
        var clock = new TestTimeProvider(DateTimeOffset.Parse("2026-09-02T10:00:00Z"));
        await using ServiceProvider provider = Build(builder =>
        {
            builder.AddStateStore<V8MemoryStore>();
            builder.AddToken<Payload>("share", token => token
                .UseServerControlledExpiration()
                .AcceptV8Tokens(v8 =>
                {
                    v8.Secret = "legacy-secret";
                    v8.ExpirationMode = BewitExpirationMode.ServerControlled;
                    v8.PayloadTypeName = typeof(Payload).FullName;
                    v8.AcceptUntil = DateTimeOffset.MaxValue;
                })
                .AddValidationPolicy<ExtendLegacyTokenPolicy>());
        }, timeProvider: clock);
        Guid nonce = Guid.NewGuid();
        var reference = new BewitTokenReference(BewitTokenFormat.V8, "share", nonce);
        await provider.GetRequiredService<IBewitTokenRepository<Payload>>().CreateAsync(
            new BewitTokenRecord(
                reference, clock.GetUtcNow().AddMinutes(-1), BewitTokenUsage.Reusable,
                BewitTokenStatus.Active, null, clock.GetUtcNow().AddDays(-7)), default);
        string token = CreateLegacyToken(nonce, new Payload("legacy"), "legacy-secret", null);

        Payload payload = await provider.GetRequiredService<IBewitTokenValidator<Payload>>()
            .ValidateAsync(new BewitToken<Payload>(token));

        payload.Value.Should().Be("legacy");
        (await provider.GetRequiredService<IBewitTokenRepository<Payload>>()
            .GetAsync(reference, default))!.ExpiresAt.Should().BeAfter(clock.GetUtcNow());
    }

    [Fact]
    public async Task ObserverFailure_DoesNotChangeValidationOutcome()
    {
        await using ServiceProvider provider = Build(builder =>
            builder.AddToken<Payload>("download", token =>
                token.AddValidationObserver<ThrowingObserver>()));
        BewitToken<Payload> token = await provider.GetRequiredService<IBewitTokenGenerator<Payload>>()
            .GenerateAsync(new Payload("document-1"));

        Payload payload = await provider.GetRequiredService<IBewitTokenValidator<Payload>>()
            .ValidateAsync(token);

        payload.Value.Should().Be("document-1");
    }

    [Fact]
    public async Task ExpiredObserver_ReceivesTypedContext()
    {
        var clock = new TestTimeProvider(DateTimeOffset.Parse("2026-09-02T10:00:00Z"));
        var events = new ObserverEvents();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton(events);
        services.AddBewit(builder =>
        {
            builder.UseSigningKey("current", Key);
            builder.AddToken<Payload>("download", token => token
                .Configure(options => options.Lifetime = TimeSpan.FromMinutes(1))
                .AddValidationObserver<RecordingObserver>());
        });
        await using ServiceProvider provider = services.BuildServiceProvider();
        BewitToken<Payload> token = await provider.GetRequiredService<IBewitTokenGenerator<Payload>>()
            .GenerateAsync(new Payload("document-1"));
        clock.Advance(TimeSpan.FromMinutes(2));

        Func<Task> action = async () => await provider
            .GetRequiredService<IBewitTokenValidator<Payload>>().ValidateAsync(token);

        await action.Should().ThrowAsync<BewitExpiredException>();
        events.Expired.Should().ContainSingle().Which.Should().Match<BewitTokenValidationContext<Payload>>(
            context => context.Payload.Value == "document-1"
                && context.Reference.Purpose == "download"
                && context.ExpirationMode == BewitExpirationMode.SelfContained);
    }

    private static ServiceProvider Build(
        Action<BewitBuilder> registration,
        Action<BewitOptions>? configure = null,
        TimeProvider? timeProvider = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        if (timeProvider is not null)
            services.AddSingleton(timeProvider);
        services.AddBewit(builder =>
        {
            builder.Configure(options =>
            {
                options.CurrentKeyId = "current";
                options.SigningKeys["current"] = Key;
                configure?.Invoke(options);
            });
            registration(builder);
        });
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    private static string CreateLegacyToken(
        Guid nonce,
        Payload payload,
        string secret,
        DateTime? expiration)
    {
        var signed = new { nonce, expirationDate = expiration, payload, type = typeof(Payload).FullName };
        string hash = Convert.ToBase64String(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signed))));
        var token = new
        {
            token = new { nonce, expirationDate = expiration },
            payload,
            hash
        };
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(token)));
    }

    public sealed record Payload(string Value);

    public sealed class TestTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan value) => _now += value;
    }

    public sealed class V9MemoryStore : MemoryStore
    {
        public override BewitTokenFormat Format => BewitTokenFormat.V9;
    }

    public sealed class V8MemoryStore : MemoryStore
    {
        public override BewitTokenFormat Format => BewitTokenFormat.V8;
    }

    public abstract class MemoryStore : IBewitTokenStateStore
    {
        private readonly Dictionary<Guid, BewitTokenRecord> _records = [];
        public abstract BewitTokenFormat Format { get; }
        public bool SupportsPurpose(string purpose) => true;
        public ValueTask CreateAsync(BewitTokenRecord record, CancellationToken cancellationToken)
        {
            _records.Add(record.Reference.TokenId, record);
            return ValueTask.CompletedTask;
        }
        public ValueTask<BewitTokenRecord?> GetAsync(BewitTokenReference reference, CancellationToken cancellationToken) =>
            ValueTask.FromResult(_records.GetValueOrDefault(reference.TokenId));
        public ValueTask<BewitTokenConsumeResult> TryConsumeAsync(BewitTokenReference reference, DateTimeOffset consumedAt, CancellationToken cancellationToken)
        {
            BewitTokenRecord? record = _records.GetValueOrDefault(reference.TokenId);
            BewitTokenConsumeStatus status = record switch
            {
                null => BewitTokenConsumeStatus.NotFound,
                { Status: BewitTokenStatus.Consumed } => BewitTokenConsumeStatus.AlreadyConsumed,
                { Status: BewitTokenStatus.Revoked } => BewitTokenConsumeStatus.Revoked,
                { ExpiresAt: var expiry } when expiry <= consumedAt => BewitTokenConsumeStatus.Expired,
                _ => BewitTokenConsumeStatus.Consumed
            };
            if (status == BewitTokenConsumeStatus.Consumed)
            {
                record = record! with { Status = BewitTokenStatus.Consumed, ConsumedAt = consumedAt };
                _records[reference.TokenId] = record;
            }
            return ValueTask.FromResult(new BewitTokenConsumeResult(status, record));
        }
        public ValueTask<bool> UpdateExpirationAsync(BewitTokenReference reference, DateTimeOffset expiration, CancellationToken cancellationToken)
        {
            if (!_records.TryGetValue(reference.TokenId, out BewitTokenRecord? record)
                || record.Status != BewitTokenStatus.Active)
                return ValueTask.FromResult(false);
            _records[reference.TokenId] = record with { ExpiresAt = expiration };
            return ValueTask.FromResult(true);
        }
        public ValueTask<long> UpdateExpirationByIdentifierAsync(string purpose, string identifier, DateTimeOffset expiration, CancellationToken cancellationToken)
        {
            Guid[] ids = _records.Values.Where(x => x.Reference.Purpose == purpose
                && x.Identifier == identifier && x.Status == BewitTokenStatus.Active)
                .Select(x => x.Reference.TokenId).ToArray();
            foreach (Guid id in ids)
                _records[id] = _records[id] with { ExpiresAt = expiration };
            return ValueTask.FromResult((long)ids.Length);
        }
        public ValueTask<long> RevokeByIdentifierAsync(string purpose, string identifier, DateTimeOffset revokedAt, CancellationToken cancellationToken)
        {
            Guid[] ids = _records.Values.Where(x => x.Reference.Purpose == purpose
                && x.Identifier == identifier && x.Status == BewitTokenStatus.Active)
                .Select(x => x.Reference.TokenId).ToArray();
            foreach (Guid id in ids)
                _records[id] = _records[id] with
                { Status = BewitTokenStatus.Revoked, RevokedAt = revokedAt };
            return ValueTask.FromResult((long)ids.Length);
        }
    }

    public sealed class ExtendLegacyTokenPolicy(
        IBewitTokenRepository<Payload> repository,
        TimeProvider timeProvider) : IBewitTokenValidationPolicy<Payload>
    {
        public async ValueTask<BewitTokenValidationPolicyResult> OnValidatingAsync(
            BewitTokenValidationContext<Payload> context,
            CancellationToken cancellationToken)
        {
            if (context.Reference.Format != BewitTokenFormat.V8 || context.Record?.Identifier is not null)
            {
                return BewitTokenValidationPolicyResult.Continue;
            }
            await repository.UpdateExpirationAsync(
                context.Reference, timeProvider.GetUtcNow().AddMinutes(5), cancellationToken);
            return BewitTokenValidationPolicyResult.RefreshState;
        }
    }

    public sealed class ThrowingObserver : IBewitTokenValidationObserver<Payload>
    {
        public ValueTask OnValidatedAsync(BewitTokenValidationContext<Payload> context, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Observability must not break authentication.");
    }

    public sealed class ObserverEvents
    {
        public List<BewitTokenValidationContext<Payload>> Expired { get; } = [];
    }

    public sealed class RecordingObserver(ObserverEvents events)
        : IBewitTokenValidationObserver<Payload>
    {
        public ValueTask OnExpiredAsync(
            BewitTokenValidationContext<Payload> context,
            CancellationToken cancellationToken)
        {
            events.Expired.Add(context);
            return ValueTask.CompletedTask;
        }
    }
}
