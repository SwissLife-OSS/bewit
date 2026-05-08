using Bewit.Exceptions;
using Bewit.Validation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bewit.Extensions.Http.Tests;

public class BewitEndpointMiddlewareTests
{
    private static IOptions<BewitTokenExtractionOptions> CreateOptions(
        Action<BewitTokenExtractionOptions>? configure = null)
    {
        var options = new BewitTokenExtractionOptions();
        configure?.Invoke(options);

        return Options.Create(options);
    }

    private static HttpContext CreateContext(
        IServiceProvider services,
        string? headerName = null,
        string? headerValue = null,
        string? queryString = null)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = services
        };

        if (headerName is not null && headerValue is not null)
        {
            context.Request.Headers[headerName] = headerValue;
        }

        if (queryString is not null)
        {
            context.Request.QueryString = new QueryString(queryString);
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_WithNoToken_ShouldReturn401()
    {
        var services = new ServiceCollection()
            .AddHttpContextAccessor()
            .BuildServiceProvider();

        var context = CreateContext(services);

        var middleware = new Bewit.Http.BewitEndpointMiddleware<string>(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithQueryToken_ShouldValidateAndCallNext()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var services = new ServiceCollection()
            .AddHttpContextAccessor()
            .AddSingleton(validatorMock.Object)
            .BuildServiceProvider();

        var context = CreateContext(services, queryString: "?bewit=valid-token");
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = context;

        var nextCalled = false;

        var middleware = new Bewit.Http.BewitEndpointMiddleware<string>(
            _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            },
            CreateOptions());

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithHeaderToken_ShouldValidateAndCallNext()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var services = new ServiceCollection()
            .AddHttpContextAccessor()
            .AddSingleton(validatorMock.Object)
            .BuildServiceProvider();

        var context = CreateContext(
            services, headerName: "bewitToken", headerValue: "header-token");

        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = context;

        var nextCalled = false;

        var middleware = new Bewit.Http.BewitEndpointMiddleware<string>(
            _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            },
            CreateOptions());

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidToken_ShouldReturn403()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BewitNotFoundException());

        var services = new ServiceCollection()
            .AddHttpContextAccessor()
            .AddSingleton(validatorMock.Object)
            .BuildServiceProvider();

        var context = CreateContext(services, queryString: "?bewit=invalid-token");
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = context;

        var middleware = new Bewit.Http.BewitEndpointMiddleware<string>(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithCustomQueryParam_ShouldReadFromCustomParam()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.IsAny<BewitToken<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("payload-value");

        var services = new ServiceCollection()
            .AddHttpContextAccessor()
            .AddSingleton(validatorMock.Object)
            .BuildServiceProvider();

        var context = CreateContext(services, queryString: "?token=custom-token");
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = context;

        var nextCalled = false;

        var middleware = new Bewit.Http.BewitEndpointMiddleware<string>(
            _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            },
            CreateOptions(o => o.QueryParamName = "token"));

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_HeaderPreferredOverQueryParam()
    {
        var validatorMock = new Mock<IBewitTokenValidator<string>>();

        validatorMock
            .Setup(v => v.ValidateBewitTokenAsync(
                It.Is<BewitToken<string>>(t => t.ToString() == "header-token"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("from-header");

        var services = new ServiceCollection()
            .AddHttpContextAccessor()
            .AddSingleton(validatorMock.Object)
            .BuildServiceProvider();

        var context = CreateContext(
            services,
            headerName: "bewitToken",
            headerValue: "header-token",
            queryString: "?bewit=query-token");

        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = context;

        var middleware = new Bewit.Http.BewitEndpointMiddleware<string>(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        validatorMock.Verify(
            v => v.ValidateBewitTokenAsync(
                It.Is<BewitToken<string>>(t => t.ToString() == "header-token"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
