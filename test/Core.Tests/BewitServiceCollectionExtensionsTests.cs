using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Tests;

public class BewitServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBewit_ShouldRegisterVariablesProvider()
    {
        var services = new ServiceCollection();

        services.AddBewit(bewit =>
        {
            bewit.ConfigureOptions(o => o.Secret = "test-secret-at-least-32-chars-long!");
            bewit.AddPayload<string>();
        });

        var sp = services.BuildServiceProvider();
        var provider = sp.GetService<IVariablesProvider>();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<VariablesProvider>();
    }

    [Fact]
    public void AddBewit_MultiplePayloads_ShouldRegisterKeyedNonceRepositories()
    {
        var services = new ServiceCollection();

        services.AddBewit(bewit =>
        {
            bewit.ConfigureOptions(o => o.Secret = "test-secret-at-least-32-chars-long!");
            bewit.AddPayload<string>();
            bewit.AddPayload<int>();
        });

        var sp = services.BuildServiceProvider();

        string stringKey = typeof(string).FullName!;
        string intKey = typeof(int).FullName!;

        var stringRepo = sp.GetKeyedService<INonceRepository>(stringKey);
        var intRepo = sp.GetKeyedService<INonceRepository>(intKey);

        stringRepo.Should().NotBeNull();
        intRepo.Should().NotBeNull();
    }
}
