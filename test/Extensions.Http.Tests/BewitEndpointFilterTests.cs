using Bewit.Exceptions;
using Bewit.Validation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bewit.Extensions.Http.Tests;

public class BewitEndpointFilterTests
{
    private static IOptions<BewitTokenExtractionOptions> CreateOptions(
        Action<BewitTokenExtractionOptions>? configure = null)
    {
        var options = new BewitTokenExtractionOptions();
        configure?.Invoke(options);

        return Options.Create(options);
    }

    private static (DefaultHttpContext Context, ServiceProvider Services) CreateContext(
        IBewitTokenValidator<string>? validator = null,
        string? headerName = null,
        string? headerValue = null,
        string? queryString = null,
        Action<BewitTokenExtractionOptions>? configureOptions = null)
    {
        var serviceCollection = new ServiceCollection()
            .AddHttpContextAccessor()
            .AddSingleton(CreateOptions(configureOptions));

        if (validator is not null)
        {
            serviceCollection.AddSingleton(validator);
        }

        var services = serviceCollection.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };

        if (headerName is not null && headerValue is not null)
        {
            context.Request.Headers[headerName] = headerValue;
        }

        if (queryString is not null)
        {
            context.Request.QueryString = new QueryString(queryString);
        }

        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = context;

        return (context, services);
    }

    [Fact]
    public async Task InvokeAsync_WithNoToken_ShouldReturnUnauthorized()
    {
        var (context, _) = CreateContext();

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);

        var result = await filter.InvokeAsync(
            invocationContext, _ => ValueTask.FromResult<object?>(Results.Ok()));

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_WithHeaderToken_ShouldValidateAndCallNext()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var (context, _) = CreateContext(
            validator: validatorMock.Object,
            headerName: "bewitToken",
            headerValue: "valid-token");

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            invocationContext,
            _ =>
            {
                nextCalled = true;

                return ValueTask.FromResult<object?>(Results.Ok());
            });

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithQueryToken_ShouldValidateAndCallNext()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var (context, _) = CreateContext(
            validator: validatorMock.Object,
            queryString: "?bewit=valid-token");

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            invocationContext,
            _ =>
            {
                nextCalled = true;

                return ValueTask.FromResult<object?>(Results.Ok());
            });

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithPreExtractedToken_ShouldValidateAndCallNext()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var (context, _) = CreateContext(validator: validatorMock.Object);
        context.Items["bewitToken"] = "pre-extracted-token";

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            invocationContext,
            _ =>
            {
                nextCalled = true;

                return ValueTask.FromResult<object?>(Results.Ok());
            });

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidToken_ShouldReturnForbid()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BewitNotFoundException());

        var (context, _) = CreateContext(
            validator: validatorMock.Object,
            queryString: "?bewit=invalid-token");

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);

        var result = await filter.InvokeAsync(
            invocationContext, _ => ValueTask.FromResult<object?>(Results.Ok()));

        result.Should().BeOfType<ForbidHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_WithCustomQueryParam_ShouldReadFromCustomParam()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var (context, _) = CreateContext(
            validator: validatorMock.Object,
            queryString: "?token=custom-token",
            configureOptions: o => o.QueryParamName = "token");

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);
        var nextCalled = false;

        var result = await filter.InvokeAsync(
            invocationContext,
            _ =>
            {
                nextCalled = true;

                return ValueTask.FromResult<object?>(Results.Ok());
            });

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithValidToken_ShouldSetPayloadOnAccessor()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("expected-payload");

        var (context, services) = CreateContext(
            validator: validatorMock.Object,
            headerName: "bewitToken",
            headerValue: "valid-token");

        var filter = new Bewit.Http.BewitEndpointFilter<string>();
        var invocationContext = new DefaultEndpointFilterInvocationContext(context);

        await filter.InvokeAsync(
            invocationContext, _ => ValueTask.FromResult<object?>(Results.Ok()));

        var accessor = services.GetRequiredService<IHttpContextAccessor>();
        var payload = accessor.GetBewitPayload<string>();
        payload.Should().Be("expected-payload");
    }
}
