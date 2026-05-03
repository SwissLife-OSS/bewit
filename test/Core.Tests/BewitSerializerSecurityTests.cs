using FluentAssertions;
using Xunit;

namespace Bewit.Tests;

public class BewitSerializerSecurityTests
{
    [Fact]
    public void Deserialize_InvalidBase64_ShouldReturnNull()
    {
        Bewit<string>? result = BewitSerializer.Deserialize<string>("not-valid-base64!!!");

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldReturnNull()
    {
        string base64 = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes("{ invalid json }"));

        Bewit<string>? result = BewitSerializer.Deserialize<string>(base64);

        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyString_ShouldReturnNull()
    {
        Bewit<string>? result = BewitSerializer.Deserialize<string>(string.Empty);

        result.Should().BeNull();
    }
}
