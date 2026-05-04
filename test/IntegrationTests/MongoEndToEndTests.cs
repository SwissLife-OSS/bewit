using Bewit.Generation;
using Bewit.Storage.MongoDB;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.IntegrationTests;

[Collection(TestCollectionNames.Integration)]
public class MongoEndToEndTests(MongoReplicaSetResource mongoResource)
{
    [Fact]
    public async Task BindConfiguration_PerPayloadOverride()
    {
        IMongoDatabase database = mongoResource.CreateDatabase();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bewit:Secret"] = "global-secret-at-least-32-chars-long",
                ["Bewit:TokenDuration"] = "00:05:00",
                ["Bewit:Custom:Secret"] = "custom-secret-at-least-32-chars-long",
                ["Bewit:Custom:TokenDuration"] = "00:30:00",
                ["Bewit:Custom:ExpiryMode"] = "ServerControlled"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddBewit(bewit =>
        {
            bewit.BindConfiguration("Bewit");
            bewit.AddPayload<string>();
            bewit.AddPayload<int>(p =>
            {
                p.BindConfiguration("Bewit:Custom");
                p.UseMongoDb(
                    _ => database,
                    m => m.NonceUsage = NonceUsage.ReUse);
            });
        });

        services.AddBewitGeneration<string>();
        services.AddBewitGeneration<int>();

        var sp = services.BuildServiceProvider();

        var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<BewitOptions>>();

        BewitOptions stringOpts = optionsMonitor.Get(typeof(string).FullName!);
        stringOpts.Secret.Should().Be("global-secret-at-least-32-chars-long");
        stringOpts.TokenDuration.Should().Be(TimeSpan.FromMinutes(5));
        stringOpts.ExpiryMode.Should().Be(ExpiryMode.SelfContained);

        BewitOptions intOpts = optionsMonitor.Get(typeof(int).FullName!);
        intOpts.Secret.Should().Be("custom-secret-at-least-32-chars-long");
        intOpts.TokenDuration.Should().Be(TimeSpan.FromMinutes(30));
        intOpts.ExpiryMode.Should().Be(ExpiryMode.ServerControlled);
    }

    [Fact]
    public async Task ServerControlled_GenerateAndValidate_ShouldRoundTrip()
    {
        IMongoDatabase database = mongoResource.CreateDatabase();

        var services = new ServiceCollection();

        services.AddBewit(bewit =>
        {
            bewit.ConfigureOptions(o =>
            {
                o.Secret = "a-very-secret-key-at-least-32-chars!";
                o.TokenDuration = TimeSpan.FromMinutes(5);
                o.ExpiryMode = ExpiryMode.ServerControlled;
            });

            bewit.UseMongoDb(
                _ => database,
                m => m.NonceUsage = NonceUsage.ReUse);

            bewit.AddPayload<string>();
        });

        services.AddBewitGeneration<string>();
        services.AddBewitValidation<string>();

        var sp = services.BuildServiceProvider();

        var generator = sp.GetRequiredService<IBewitTokenGenerator<string>>();
        var validator = sp.GetRequiredService<Bewit.Validation.IBewitTokenValidator<string>>();

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "server-controlled-payload", null, CancellationToken.None);

        string payload = await validator.ValidateBewitTokenAsync(
            token, CancellationToken.None);

        payload.Should().Be("server-controlled-payload");
    }

    [Fact]
    public async Task Revoker_ShouldInvalidateTokens()
    {
        IMongoDatabase database = mongoResource.CreateDatabase();

        var services = new ServiceCollection();

        services.AddBewit(bewit =>
        {
            bewit.ConfigureOptions(o =>
            {
                o.Secret = "a-very-secret-key-at-least-32-chars!";
                o.TokenDuration = TimeSpan.FromMinutes(5);
                o.ExpiryMode = ExpiryMode.ServerControlled;
            });

            bewit.UseMongoDb(
                _ => database,
                m => m.NonceUsage = NonceUsage.ReUse);

            bewit.AddPayload<string>();
        });

        services.AddBewitGeneration<string>();
        services.AddBewitValidation<string>();

        var sp = services.BuildServiceProvider();

        var generator = sp.GetRequiredService<IBewitTokenGenerator<string>>();
        var validator = sp.GetRequiredService<Bewit.Validation.IBewitTokenValidator<string>>();
        var revoker = sp.GetRequiredService<IBewitTokenRevoker<string>>();

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "revoke-me",
            new BewitTokenOptions { Identifier = "user-123" },
            CancellationToken.None);

        (await validator.ValidateBewitTokenAsync(token, CancellationToken.None))
            .Should().Be("revoke-me");

        await revoker.RevokeByIdentifierAsync("user-123", CancellationToken.None);

        Func<Task> act = () => validator
            .ValidateBewitTokenAsync(token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<Exception>();
    }
}
