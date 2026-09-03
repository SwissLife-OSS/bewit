using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bewit.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.IntegrationTests;

[Collection(IntegrationFixture.Name)]
public sealed class MongoV9EndToEndTests(MongoReplicaSetResource mongo)
{
    private const string Key = "a-production-quality-secret-with-at-least-32-bytes";

    [Fact]
    public async Task ServerControlledToken_RoundTrips()
    {
        await using ServiceProvider provider = Build(mongo.CreateDatabase(), BewitTokenUsage.Reusable);
        BewitToken<string> token = await provider.GetRequiredService<IBewitTokenGenerator<string>>()
            .GenerateAsync("payload", new BewitTokenOptions { Identifier = "share-1" });

        string payload = await provider.GetRequiredService<IBewitTokenValidator<string>>()
            .ValidateAsync(token);

        payload.Should().Be("payload");
    }

    [Fact]
    public async Task ExpiredToken_CanBeExtendedByIdentifier()
    {
        await using ServiceProvider provider = Build(
            mongo.CreateDatabase(), BewitTokenUsage.Reusable, TimeSpan.FromMilliseconds(20));
        BewitToken<string> token = await provider.GetRequiredService<IBewitTokenGenerator<string>>()
            .GenerateAsync("payload", new BewitTokenOptions { Identifier = "share-1" });
        await Task.Delay(50);
        IBewitTokenValidator<string> validator = provider.GetRequiredService<IBewitTokenValidator<string>>();
        Func<Task> expired = async () => await validator.ValidateAsync(token);
        await expired.Should().ThrowAsync<BewitExpiredException>();

        BewitBulkOperationResult result = await provider
            .GetRequiredService<IBewitTokenRepository<string>>()
            .UpdateExpirationByIdentifierAsync(
                "share-1", DateTimeOffset.UtcNow.AddMinutes(1), default);

        result.GetAffectedCount(BewitTokenFormat.V9).Should().Be(1);
        (await validator.ValidateAsync(token)).Should().Be("payload");
    }

    [Fact]
    public async Task SingleUseToken_CannotBeConsumedTwice()
    {
        await using ServiceProvider provider = Build(mongo.CreateDatabase(), BewitTokenUsage.SingleUse);
        BewitToken<string> token = await provider.GetRequiredService<IBewitTokenGenerator<string>>()
            .GenerateAsync("payload");
        IBewitTokenValidator<string> validator = provider.GetRequiredService<IBewitTokenValidator<string>>();

        await validator.ValidateAsync(token);
        Func<Task> second = async () => await validator.ValidateAsync(token);

        await second.Should().ThrowAsync<BewitAlreadyConsumedException>();
    }

    [Fact]
    public async Task V8Compatibility_ReadsAndUpdatesExistingLegacyCollection()
    {
        IMongoDatabase database = mongo.CreateDatabase();
        Guid nonce = Guid.NewGuid();
        await database.GetCollection<BsonDocument>("bewit_nonces").InsertOneAsync(new BsonDocument
        {
            ["_id"] = nonce.ToString(),
            ["exp"] = DateTime.UtcNow.AddMinutes(-1),
            ["ident"] = "legacy-share",
            ["del"] = false,
            ["createdAt"] = DateTime.UtcNow.AddDays(-7)
        });
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBewit(builder =>
        {
            builder.UseSigningKey("current", Key);
            builder.UseMongoDb(_ => database);
            builder.UseV8MongoDb("share", options => options.Usage = BewitTokenUsage.Reusable);
            builder.AddToken<string>("share", token => token
                .UseServerControlledExpiration()
                .AcceptV8Tokens(options =>
                {
                    options.Secret = "legacy-secret";
                    options.ExpirationMode = BewitExpirationMode.ServerControlled;
                    options.PayloadTypeName = typeof(string).FullName;
                    options.AcceptUntil = DateTimeOffset.UtcNow.AddDays(1);
                }));
        });
        await using ServiceProvider provider = services.BuildServiceProvider();
        IBewitTokenRepository<string> repository = provider.GetRequiredService<IBewitTokenRepository<string>>();

        BewitBulkOperationResult result = await repository.UpdateExpirationByIdentifierAsync(
            "legacy-share", DateTimeOffset.UtcNow.AddMinutes(5), default);
        string payload = await provider.GetRequiredService<IBewitTokenValidator<string>>()
            .ValidateAsync(new BewitToken<string>(CreateLegacyToken(nonce, "legacy", "legacy-secret")));

        result.GetAffectedCount(BewitTokenFormat.V8).Should().Be(1);
        result.GetAffectedCount(BewitTokenFormat.V9).Should().Be(0);
        payload.Should().Be("legacy");
    }

    private static ServiceProvider Build(
        IMongoDatabase database,
        BewitTokenUsage usage,
        TimeSpan? lifetime = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBewit(builder =>
        {
            builder.UseSigningKey("current", Key);
            builder.UseMongoDb(_ => database);
            builder.AddToken<string>("share", token => token
                .UseServerControlledExpiration()
                .Configure(options =>
                {
                    options.Usage = usage;
                    options.Lifetime = lifetime ?? TimeSpan.FromMinutes(5);
                }));
        });
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    private static string CreateLegacyToken(Guid nonce, string payload, string secret)
    {
        var signed = new
        {
            nonce,
            expirationDate = (DateTime?)null,
            payload,
            type = typeof(string).FullName
        };
        string hash = Convert.ToBase64String(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(signed))));
        var value = new
        {
            token = new { nonce, expirationDate = (DateTime?)null },
            payload,
            hash
        };
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));
    }
}
