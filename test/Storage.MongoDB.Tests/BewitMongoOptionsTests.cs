using FluentAssertions;
using Xunit;

namespace Bewit.Storage.MongoDB.Tests;

public class BewitMongoOptionsTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var options = new BewitMongoOptions();

        options.CollectionName.Should().Be("bewit_nonces");
        options.AuthType.Should().Be(MongoAuthType.Password);
        options.NonceUsage.Should().Be(NonceUsage.OneTime);
        options.RecordExpireAfterDays.Should().Be(730);
    }

    [Fact]
    public void MongoAuthType_Values_ShouldMatchMongoExtensions()
    {
        ((int)MongoAuthType.Password).Should().Be(0);
        ((int)MongoAuthType.Oidc).Should().Be(1);
    }
}
