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
        var options = CreateOptions(ExpiryMode.SelfContained);
        var generator = CreateGenerator(options);

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
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var generator = CreateGenerator(options);

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
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var generator = CreateGenerator(options);

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
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var generator = CreateGenerator(options);

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
        var options = CreateOptions(ExpiryMode.SelfContained);
        var generator = CreateGenerator(options);

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
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var generator = CreateGenerator(options);

        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "test-payload", null, CancellationToken.None);

        Bewit<string>? bewit = BewitSerializer.Deserialize<string>((string)token);

        bewit.Should().NotBeNull();
        bewit!.Token.ExpirationDate.Should().BeNull();
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
