using Bewit.Generation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bewit.IntegrationTests;

public class EndToEndTests
{
    [Fact]
    public async Task SelfContained_GenerateAndValidate_ShouldRoundTrip()
    {
        var services = new ServiceCollection();

        services.AddBewit(bewit =>
        {
            bewit.ConfigureOptions(o =>
            {
                o.Secret = "a-very-secret-key-at-least-32-chars!";
                o.TokenDuration = TimeSpan.FromMinutes(5);
                o.ExpiryMode = ExpiryMode.SelfContained;
            });
            bewit.AddPayload<string>();
        });

        services.AddBewitGeneration<string>();
        services.AddBewitValidation<string>();

        var sp = services.BuildServiceProvider();

        var generator = sp.GetRequiredService<IBewitTokenGenerator<string>>();
        var validator = sp.GetRequiredService<Bewit.Validation.IBewitTokenValidator<string>>();

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "integration-payload", null, CancellationToken.None);

        string payload = await validator.ValidateBewitTokenAsync(
            token, CancellationToken.None);

        payload.Should().Be("integration-payload");
    }

    [Fact]
    public async Task MultiPayload_DifferentSecrets_ShouldWork()
    {
        var services = new ServiceCollection();

        services.AddBewit(bewit =>
        {
            bewit.AddPayload<string>(p =>
                p.ConfigureOptions(o =>
                {
                    o.Secret = "string-secret-at-least-32-chars-long!";
                    o.TokenDuration = TimeSpan.FromMinutes(1);
                }));

            bewit.AddPayload<int>(p =>
                p.ConfigureOptions(o =>
                {
                    o.Secret = "int-secret-at-least-32-chars-long!!!";
                    o.TokenDuration = TimeSpan.FromMinutes(10);
                }));
        });

        services.AddBewitGeneration<string>();
        services.AddBewitGeneration<int>();
        services.AddBewitValidation<string>();
        services.AddBewitValidation<int>();

        var sp = services.BuildServiceProvider();

        var stringGen = sp.GetRequiredService<IBewitTokenGenerator<string>>();
        var intGen = sp.GetRequiredService<IBewitTokenGenerator<int>>();
        var stringVal = sp.GetRequiredService<Bewit.Validation.IBewitTokenValidator<string>>();
        var intVal = sp.GetRequiredService<Bewit.Validation.IBewitTokenValidator<int>>();

        BewitToken<string> strToken = await stringGen.GenerateBewitTokenAsync(
            "hello", null, CancellationToken.None);

        BewitToken<int> intToken = await intGen.GenerateBewitTokenAsync(
            42, null, CancellationToken.None);

        (await stringVal.ValidateBewitTokenAsync(strToken, CancellationToken.None))
            .Should().Be("hello");

        (await intVal.ValidateBewitTokenAsync(intToken, CancellationToken.None))
            .Should().Be(42);
    }

    [Fact]
    public async Task BindConfiguration_ShouldReadFromAppSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bewit:Secret"] = "bound-secret-at-least-32-chars-long!",
                ["Bewit:TokenDuration"] = "00:10:00",
                ["Bewit:ExpiryMode"] = "SelfContained"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddBewit(bewit =>
        {
            bewit.BindConfiguration("Bewit");
            bewit.AddPayload<string>();
        });

        services.AddBewitGeneration<string>();
        services.AddBewitValidation<string>();

        var sp = services.BuildServiceProvider();

        var generator = sp.GetRequiredService<IBewitTokenGenerator<string>>();
        var validator = sp.GetRequiredService<Bewit.Validation.IBewitTokenValidator<string>>();

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "config-payload", null, CancellationToken.None);

        string payload = await validator.ValidateBewitTokenAsync(
            token, CancellationToken.None);

        payload.Should().Be("config-payload");
    }

    [Fact]
    public async Task BindConfiguration_CodeOverridesConfig()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Bewit:Secret"] = "from-config-at-least-32-chars-long!",
                ["Bewit:TokenDuration"] = "00:01:00"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddBewit(bewit =>
        {
            bewit.BindConfiguration("Bewit");
            bewit.ConfigureOptions(o =>
            {
                o.TokenDuration = TimeSpan.FromMinutes(30);
            });
            bewit.AddPayload<string>();
        });

        services.AddBewitGeneration<string>();

        var sp = services.BuildServiceProvider();

        string optionsName = typeof(string).FullName!;
        var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<BewitOptions>>();
        BewitOptions resolved = optionsMonitor.Get(optionsName);

        resolved.Secret.Should().Be("from-config-at-least-32-chars-long!");
        resolved.TokenDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task BindConfiguration_PerPayloadOverride()
    {
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
            bewit.AddPayload<int>(p => p.BindConfiguration("Bewit:Custom"));
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
}
