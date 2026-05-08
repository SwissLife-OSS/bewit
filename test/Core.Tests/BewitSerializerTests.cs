using FluentAssertions;
using Xunit;

namespace Bewit.Tests;

public class BewitSerializerTests
{
    [Fact]
    public void Serialize_ThenDeserialize_ShouldRoundTrip()
    {
        var token = Token.Create(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));
        var original = new Bewit<string>(token, "test-payload", "test-hash");

        string serialized = BewitSerializer.Serialize(original);
        Bewit<string>? deserialized = BewitSerializer.Deserialize<string>(serialized);

        deserialized.Should().NotBeNull();
        deserialized!.Token.Nonce.Should().Be(original.Token.Nonce);
        deserialized.Payload.Should().Be("test-payload");
        deserialized.Hash.Should().Be("test-hash");
    }

    [Fact]
    public void Serialize_ShouldProduceBase64String()
    {
        var token = Token.Create(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));
        var bewit = new Bewit<string>(token, "payload", "hash");

        string serialized = BewitSerializer.Serialize(bewit);

        var act = () => Convert.FromBase64String(serialized);
        act.Should().NotThrow();
    }

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
            System.Text.Encoding.UTF8.GetBytes("not json"));

        Bewit<string>? result = BewitSerializer.Deserialize<string>(base64);

        result.Should().BeNull();
    }
}
