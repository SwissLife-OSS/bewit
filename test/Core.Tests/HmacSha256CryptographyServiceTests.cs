using FluentAssertions;
using Xunit;

namespace Bewit.Tests;

public class HmacSha256CryptographyServiceTests
{
    [Fact]
    public void GetHash_ShouldReturnConsistentHash()
    {
        var service = new HmacSha256CryptographyService("test-secret");
        var nonce = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var expiry = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        string hash1 = service.GetHash(nonce, expiry, "payload");
        string hash2 = service.GetHash(nonce, expiry, "payload");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHash_DifferentPayloads_ShouldReturnDifferentHashes()
    {
        var service = new HmacSha256CryptographyService("test-secret");
        var nonce = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var expiry = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        string hash1 = service.GetHash(nonce, expiry, "payload-a");
        string hash2 = service.GetHash(nonce, expiry, "payload-b");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetHash_DifferentSecrets_ShouldReturnDifferentHashes()
    {
        var service1 = new HmacSha256CryptographyService("secret-1");
        var service2 = new HmacSha256CryptographyService("secret-2");
        var nonce = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var expiry = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        string hash1 = service1.GetHash(nonce, expiry, "payload");
        string hash2 = service2.GetHash(nonce, expiry, "payload");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetHash_NullExpiry_ShouldSucceed()
    {
        var service = new HmacSha256CryptographyService("test-secret");
        var nonce = Guid.Parse("12345678-1234-1234-1234-123456789012");

        string hash = service.GetHash<string>(nonce, null, "payload");

        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptySecret_ShouldThrow(string secret)
    {
        var act = () => new HmacSha256CryptographyService(secret);

        act.Should().Throw<ArgumentException>();
    }
}
