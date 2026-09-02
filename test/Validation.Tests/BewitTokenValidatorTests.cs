using Bewit.Exceptions;
using Bewit.Generation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task AddBewitValidation_RegisteredEvents_ShouldReceiveValidationEvents()
    {
        var validationEvents = new Mock<IBewitTokenValidationEvents<string>>();
        var variables = CreateVariablesProvider(FixedUtcNow);
        var services = new ServiceCollection();
        INonceRepository? suppliedNonceRepository = null;

        services.AddSingleton(variables);
        services.AddBewit(bewit =>
        {
            bewit.ConfigureOptions(options =>
            {
                options.Secret = Secret;
                options.TokenDuration = TimeSpan.FromMinutes(5);
                options.ExpiryMode = ExpiryMode.SelfContained;
            });
            bewit.AddPayload<string>();
        });
        services.AddBewitTokenValidationEvents<string>((_, nonceRepository) =>
        {
            suppliedNonceRepository = nonceRepository;
            return validationEvents.Object;
        });
        services.AddBewitGeneration<string>();
        services.AddBewitValidation<string>();

        await using ServiceProvider provider = services.BuildServiceProvider();
        var generator = provider.GetRequiredService<IBewitTokenGenerator<string>>();
        var validator = provider.GetRequiredService<IBewitTokenValidator<string>>();
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload",
            new BewitTokenOptions { Duration = TimeSpan.FromMinutes(-1) },
            CancellationToken.None);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitExpiredException>();
        suppliedNonceRepository.Should().BeOfType<DefaultNonceRepository>();
        validationEvents.Verify(
            o => o.OnValidatingAsync(
                It.Is<BewitTokenValidatingContext<string>>(context =>
                    context.Payload == "payload"
                    && context.Nonce == FixedNonce
                    && context.ValidatedAt == FixedUtcNow
                    && context.ExpiryMode == ExpiryMode.SelfContained
                    && context.TokenExpirationDate == FixedUtcNow.AddMinutes(-1)),
                CancellationToken.None),
            Times.Once);
        validationEvents.Verify(
            o => o.OnExpiredAsync(
                It.Is<BewitTokenExpiredContext<string>>(context =>
                    context.Payload == "payload"
                    && context.ExpirationDate == FixedUtcNow.AddMinutes(-1)),
                CancellationToken.None),
            Times.Once);
    }

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
        DateTime expirationDate = FixedUtcNow.AddMinutes(1);

        var generateTime = CreateVariablesProvider(FixedUtcNow);
        var generator = CreateGenerator(options, new DefaultNonceRepository(), generateTime);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);

        DateTime validatedAt = FixedUtcNow.AddMinutes(2);
        var validateTime = CreateVariablesProvider(validatedAt);
        var validationEvents = new Mock<IBewitTokenValidationEvents<string>>();
        BewitTokenExpiredContext<string>? observedContext = null;

        validationEvents
            .Setup(o => o.OnExpiredAsync(
                It.IsAny<BewitTokenExpiredContext<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<BewitTokenExpiredContext<string>, CancellationToken>(
                (context, _) => observedContext = context)
            .Returns(ValueTask.CompletedTask);

        var validator = CreateValidator(
            options,
            new DefaultNonceRepository(),
            validateTime,
            [validationEvents.Object]);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitExpiredException>();
        observedContext.Should().BeEquivalentTo(new BewitTokenExpiredContext<string>(
            "payload",
            FixedNonce,
            expirationDate,
            validatedAt,
            ExpiryMode.SelfContained));
    }

    [Fact]
    public async Task ValidateBewitToken_ExpiredToken_ShouldInvokeEventsInRegistrationOrder()
    {
        var options = CreateOptions(ExpiryMode.SelfContained, TimeSpan.FromMinutes(-1));
        var variables = CreateVariablesProvider(FixedUtcNow);
        var generator = CreateGenerator(
            options, new DefaultNonceRepository(), variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);
        var calls = new List<string>();
        var validator = CreateValidator(
            options,
            new DefaultNonceRepository(),
            variables,
            [
                new RecordingValidationEvents("first", calls),
                new RecordingValidationEvents("second", calls)
            ]);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitExpiredException>();
        calls.Should().Equal(
            "first:validating",
            "second:validating",
            "first:expired",
            "second:expired");
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

        var validationEvents = new Mock<IBewitTokenValidationEvents<string>>();
        var validator = CreateValidator(
            differentOptions,
            new DefaultNonceRepository(),
            variables,
            [validationEvents.Object]);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitInvalidException>();
        validationEvents.Verify(
            o => o.OnValidatingAsync(
                It.IsAny<BewitTokenValidatingContext<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        validationEvents.Verify(
            o => o.OnExpiredAsync(
                It.IsAny<BewitTokenExpiredContext<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
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
    public async Task ValidateBewitToken_ServerControlled_OnValidatingCanUpdateExpiryBeforeNonceIsLoaded()
    {
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var variables = CreateVariablesProvider(FixedUtcNow);
        var nonceRepo = new Mock<INonceRepository>();
        DateTime storedExpirationDate = FixedUtcNow.AddMinutes(-1);
        DateTime extendedExpirationDate = FixedUtcNow.AddMinutes(5);

        nonceRepo.Setup(r => r.InsertOneAsync(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        nonceRepo
            .Setup(r => r.UpdateExpiryAsync(
                FixedNonce,
                extendedExpirationDate,
                It.IsAny<CancellationToken>()))
            .Callback(() => storedExpirationDate = extendedExpirationDate)
            .ReturnsAsync(true);
        nonceRepo
            .Setup(r => r.TakeOneAsync(FixedNonce, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Token.Create(FixedNonce, storedExpirationDate));

        var generator = CreateGenerator(options, nonceRepo.Object, variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);
        var validationEvents = new ExtendingValidationEvents(
            nonceRepo.Object, extendedExpirationDate);
        var validator = CreateValidator(
            options, nonceRepo.Object, variables, [validationEvents]);

        string payload = await validator.ValidateBewitTokenAsync(
            token, CancellationToken.None);

        payload.Should().Be("payload");
        validationEvents.ValidatingContext.Should().BeEquivalentTo(
            new BewitTokenValidatingContext<string>(
                "payload",
                FixedNonce,
                FixedUtcNow,
                ExpiryMode.ServerControlled,
                null));
        validationEvents.ExpiredCallCount.Should().Be(0);
        nonceRepo.Verify(
            r => r.UpdateExpiryAsync(
                FixedNonce,
                extendedExpirationDate,
                CancellationToken.None),
            Times.Once);
        nonceRepo.Verify(
            r => r.TakeOneAsync(FixedNonce, CancellationToken.None),
            Times.Once);
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

    [Fact]
    public async Task ValidateBewitToken_ServerControlled_ExpiredToken_ShouldNotifyWithRepositoryExpiry()
    {
        var options = CreateOptions(ExpiryMode.ServerControlled);
        var variables = CreateVariablesProvider(FixedUtcNow);
        var nonceRepo = new Mock<INonceRepository>();
        DateTime expirationDate = FixedUtcNow.AddMinutes(-1);

        nonceRepo.Setup(r => r.InsertOneAsync(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        nonceRepo.Setup(r => r.TakeOneAsync(FixedNonce, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Token.Create(FixedNonce, expirationDate));

        var generator = CreateGenerator(options, nonceRepo.Object, variables);
        BewitToken<string> token = await generator.GenerateBewitTokenAsync(
            "payload", null, CancellationToken.None);

        var validationEvents = new Mock<IBewitTokenValidationEvents<string>>();
        BewitTokenExpiredContext<string>? observedContext = null;

        validationEvents
            .Setup(o => o.OnExpiredAsync(
                It.IsAny<BewitTokenExpiredContext<string>>(),
                It.IsAny<CancellationToken>()))
            .Callback<BewitTokenExpiredContext<string>, CancellationToken>(
                (context, _) => observedContext = context)
            .Returns(ValueTask.CompletedTask);

        var validator = CreateValidator(
            options,
            nonceRepo.Object,
            variables,
            [validationEvents.Object]);

        Func<Task> act = () => validator.ValidateBewitTokenAsync(
            token, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<BewitExpiredException>();
        observedContext.Should().BeEquivalentTo(new BewitTokenExpiredContext<string>(
            "payload",
            FixedNonce,
            expirationDate,
            FixedUtcNow,
            ExpiryMode.ServerControlled));
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
        IVariablesProvider variables,
        IEnumerable<IBewitTokenValidationEvents<string>>? validationEvents = null) =>
        new(options,
            new HmacSha256CryptographyService(options.Value.Secret),
            nonceRepo,
            variables,
            validationEvents ?? []);

    private sealed class ExtendingValidationEvents(
        INonceRepository nonceRepository,
        DateTime newExpirationDate) : IBewitTokenValidationEvents<string>
    {
        public BewitTokenValidatingContext<string>? ValidatingContext { get; private set; }

        public int ExpiredCallCount { get; private set; }

        public async ValueTask OnValidatingAsync(
            BewitTokenValidatingContext<string> context,
            CancellationToken cancellationToken)
        {
            ValidatingContext = context;
            await nonceRepository.UpdateExpiryAsync(
                context.Nonce, newExpirationDate, cancellationToken);
        }

        public ValueTask OnExpiredAsync(
            BewitTokenExpiredContext<string> context,
            CancellationToken cancellationToken)
        {
            ExpiredCallCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingValidationEvents(
        string name,
        ICollection<string> calls) : IBewitTokenValidationEvents<string>
    {
        public ValueTask OnValidatingAsync(
            BewitTokenValidatingContext<string> context,
            CancellationToken cancellationToken)
        {
            calls.Add($"{name}:validating");
            return ValueTask.CompletedTask;
        }

        public ValueTask OnExpiredAsync(
            BewitTokenExpiredContext<string> context,
            CancellationToken cancellationToken)
        {
            calls.Add($"{name}:expired");
            return ValueTask.CompletedTask;
        }
    }
}
