using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bewit.Generation.Tests;

public class BewitTokenGeneratorTests
{
    private readonly Mock<INonceRepository> _nonceRepository = new();
    private readonly Mock<IVariablesProvider> _variablesProvider = new();

    private static readonly DateTime FixedUtcNow = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid FixedNonce = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    public BewitTokenGeneratorTests()
    {
        _variablesProvider.Setup(v => v.UtcNow).Returns(FixedUtcNow);
        _variablesProvider.Setup(v => v.NextToken).Returns(FixedNonce);
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_SelfContained_ShouldReturnToken()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.SelfContained);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "my-payload", null, CancellationToken.None);

        ((string)token).Should().NotBeNullOrWhiteSpace();

        _nonceRepository.Verify(
            r => r.InsertOneAsync(It.IsAny<Token>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_ServerControlled_ShouldInsertNonce()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.ServerControlled);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "my-payload", null, CancellationToken.None);

        ((string)token).Should().NotBeNullOrWhiteSpace();

        _nonceRepository.Verify(
            r => r.InsertOneAsync(
                It.Is<Token>(t =>
                    t.Nonce == FixedNonce &&
                    t.ExpirationDate.HasValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_CustomDuration_ShouldOverrideDefault()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.ServerControlled);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);

        var tokenOptions = new BewitTokenOptions
        {
            Duration = TimeSpan.FromHours(2)
        };

        await generator.GenerateBewitTokenAsync("payload", tokenOptions, CancellationToken.None);

        _nonceRepository.Verify(
            r => r.InsertOneAsync(
                It.Is<Token>(t =>
                    t.ExpirationDate!.Value == FixedUtcNow.AddHours(2)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_WithIdentifier_ShouldIncludeInToken()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.ServerControlled);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);

        var tokenOptions = new BewitTokenOptions
        {
            Identifier = "user-123"
        };

        await generator.GenerateBewitTokenAsync("payload", tokenOptions, CancellationToken.None);

        _nonceRepository.Verify(
            r => r.InsertOneAsync(
                It.Is<Token>(t => t.Identifier == "user-123"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_SelfContained_TokenShouldBeDeserializable()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.SelfContained);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "test-payload", null, CancellationToken.None);

        Bewit<string>? bewit = BewitSerializer.Deserialize<string>((string)token);

        bewit.Should().NotBeNull();
        bewit!.Payload.Should().Be("test-payload");
        bewit.Token.ExpirationDate.Should().NotBeNull();
        bewit.Token.Nonce.Should().Be(FixedNonce);
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_ServerControlled_TokenShouldNotCarryExpiry()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.ServerControlled);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "test-payload", null, CancellationToken.None);

        Bewit<string>? bewit = BewitSerializer.Deserialize<string>((string)token);

        bewit.Should().NotBeNull();
        bewit!.Token.ExpirationDate.Should().BeNull();
    }

    [Fact]
    public async Task GenerateBewitTokenAsync_ServerControlled_ShouldStoreExtraProperties()
    {
        IOptions<BewitOptions> options = CreateOptions(ExpiryMode.ServerControlled);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);
        BewitTokenOptions bewitTokenOptions = new()
        {
            Identifier = "test-identifier",
            ExtraProperties = new Dictionary<string, object> { ["extra-prop-1"] = "extra-prop-1-val" }
        };

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "test-payload", bewitTokenOptions, CancellationToken.None);

        _nonceRepository.Verify(
            r => r.InsertOneAsync(
                It.Is<Token>(t => t.Identifier == "test-identifier" && t.ExtraProperties.ContainsKey("extra-prop-1")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(ExpiryMode.ServerControlled)]
    [InlineData(ExpiryMode.SelfContained)]
    public async Task GenerateBewitTokenAsync_WithTheoryExpiryMode_ShouldNeverConatainsExtraProperties(ExpiryMode expiryMode)
    {
        IOptions<BewitOptions> options = CreateOptions(expiryMode);
        IBewitTokenGenerator<string> generator = CreateGenerator(options);
        BewitTokenOptions bewitTokenOptions = new()
        {
            Identifier = "test-identifier",
            ExtraProperties = new Dictionary<string, object> { ["extra-prop-1"] = "extra-prop-1-val" }
        };
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "test-payload", bewitTokenOptions, CancellationToken.None);

        Bewit<string>? bewit = BewitSerializer.Deserialize<string>((string)token);

        bewit.Should().NotBeNull();
        bewit.Token.Identifier.Should().BeNull();
        bewit.Token.ExtraProperties.Should().BeEmpty();
    }

    private IBewitTokenGenerator<string> CreateGenerator(IOptions<BewitOptions> options)
    {
        var cryptoService = new HmacSha256CryptographyService(options.Value.Secret);

        return new BewitTokenGenerator<string>(
            options,
            cryptoService,
            _nonceRepository.Object,
            _variablesProvider.Object);
    }

    private static IOptions<BewitOptions> CreateOptions(ExpiryMode mode) =>
        Options.Create(new BewitOptions
        {
            Secret = "a-very-secret-key-at-least-32-chars!",
            TokenDuration = TimeSpan.FromMinutes(5),
            ExpiryMode = mode
        });
}
