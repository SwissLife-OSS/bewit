using Bewit.Exceptions;
using Bewit.Generation;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bewit.Validation.Tests;

public class BewitTokenValidatorTests
{
    private const string Secret = "a-very-secret-key-at-least-32-chars!";
    private static readonly DateTime FixedUtcNow = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid FixedNonce = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public async Task ValidateBewitToken_SelfContained_ValidToken_ShouldReturnPayload()
    {
        var options = CreateOptions(ExpiryMode.SelfContained);
        var variables = CreateVariablesProvider(FixedUtcNow);

        var generator = CreateGenerator(options, new DefaultNonceRepository(), variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "my-payload", null, CancellationToken.None);

        var validator = CreateValidator(options, new DefaultNonceRepository(), variables);
        string payload = await validator.ValidateBewitTokenAsync(token, CancellationToken.None);

        payload.Should().Be("my-payload");
    }

    [Fact]
    public async Task ValidateBewitToken_SelfContained_ExpiredToken_ShouldThrow()
    {
        var options = CreateOptions(ExpiryMode.SelfContained, TimeSpan.FromMinutes(1));

        var generateTime = CreateVariablesProvider(FixedUtcNow);
        var generator = CreateGenerator(options, new DefaultNonceRepository(), generateTime);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);

        var validateTime = CreateVariablesProvider(FixedUtcNow.AddMinutes(2));
        var validator = CreateValidator(options, new DefaultNonceRepository(), validateTime);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitExpiredException>();
    }

    [Fact]
    public async Task ValidateBewitToken_TamperedToken_ShouldThrow()
    {
        var options = CreateOptions(ExpiryMode.SelfContained);
        var variables = CreateVariablesProvider(FixedUtcNow);

        var generator = CreateGenerator(options, new DefaultNonceRepository(), variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "original-payload", null, CancellationToken.None);

        var differentOptions = Options.Create(new BewitOptions
        {
            Secret = "a-different-secret-key-at-least-32-chars!",
            TokenDuration = TimeSpan.FromMinutes(5),
            ExpiryMode = ExpiryMode.SelfContained
        });

        var validator = CreateValidator(differentOptions, new DefaultNonceRepository(), variables);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitInvalidException>();
    }

    [Fact]
    public async Task ValidateBewitToken_InvalidBase64_ShouldThrow()
    {
        var options = CreateOptions(ExpiryMode.SelfContained);
        var variables = CreateVariablesProvider(FixedUtcNow);
        var validator = CreateValidator(options, new DefaultNonceRepository(), variables);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            new BewitToken<string>("not-valid!!!"), CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitInvalidException>();
    }

    [Fact]
    public async Task ValidateBewitToken_ServerControlled_ValidNonce_ShouldReturnPayload()
    {
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var variables = CreateVariablesProvider(FixedUtcNow);
        var nonceRepo = new Mock<INonceRepository>();

        nonceRepo.Setup(r => r.InsertOneAsync(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        nonceRepo.Setup(r => r.TakeOneAsync(FixedNonce, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Token.Create(FixedNonce, FixedUtcNow.AddMinutes(5)));

        var generator = CreateGenerator(options, nonceRepo.Object, variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);

        var validator = CreateValidator(options, nonceRepo.Object, variables);
        string payload = await validator.ValidateBewitTokenAsync(token, CancellationToken.None);

        payload.Should().Be("payload");
    }

    [Fact]
    public async Task ValidateBewitToken_ServerControlled_NonceNotFound_ShouldThrow()
    {
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var variables = CreateVariablesProvider(FixedUtcNow);
        var nonceRepo = new Mock<INonceRepository>();

        nonceRepo.Setup(r => r.InsertOneAsync(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        nonceRepo.Setup(r => r.TakeOneAsync(FixedNonce, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Token?)null);

        var generator = CreateGenerator(options, nonceRepo.Object, variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);

        var validator = CreateValidator(options, nonceRepo.Object, variables);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitNotFoundException>();
    }

    private static IOptions<BewitOptions> CreateOptions(
        ExpiryMode mode,
        TimeSpan? duration = null) =>
        Options.Create(new BewitOptions
        {
            Secret = Secret,
            TokenDuration = duration ?? TimeSpan.FromMinutes(5),
            ExpiryMode = mode
        });

    private static IVariablesProvider CreateVariablesProvider(DateTime utcNow)
    {
        var mock = new Mock<IVariablesProvider>();
        mock.Setup(v => v.UtcNow).Returns(utcNow);
        mock.Setup(v => v.NextToken).Returns(FixedNonce);

        return mock.Object;
    }

    private static BewitTokenGenerator<string> CreateGenerator(
        IOptions<BewitOptions> options,
        INonceRepository nonceRepo,
        IVariablesProvider variables) =>
        new(options,
            new HmacSha256CryptographyService(options.Value.Secret),
            nonceRepo,
            variables);

    private static BewitTokenValidator<string> CreateValidator(
        IOptions<BewitOptions> options,
        INonceRepository nonceRepo,
        IVariablesProvider variables) =>
        new(options,
            new HmacSha256CryptographyService(options.Value.Secret),
            nonceRepo,
            variables);
}
