using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Bewit.Extensions.HotChocolate.Tests;

public class BewitTokenExtractionMiddlewareTests
{
    private static IOptions<BewitTokenExtractionOptions> CreateOptions(
        Action<BewitTokenExtractionOptions>? configure = null)
    {
        var options = new BewitTokenExtractionOptions();
        configure?.Invoke(options);

        return Options.Create(options);
    }

    [Fact]
    public async Task InvokeAsync_WithHeaderToken_ShouldSetContextItem()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["bewitToken"] = "my-token-value";

        var nextCalled = false;

        var middleware = new BewitTokenExtractionMiddleware(
            _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            },
            CreateOptions());

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Items["bewitToken"].Should().Be("my-token-value");
    }

    [Fact]
    public async Task InvokeAsync_WithQueryParam_ShouldSetContextItem()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?bewit=query-token");

        var middleware = new BewitTokenExtractionMiddleware(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        context.Items["bewitToken"].Should().Be("query-token");
    }

    [Fact]
    public async Task InvokeAsync_WithHeaderAndQueryParam_ShouldPreferHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["bewitToken"] = "header-token";
        context.Request.QueryString = new QueryString("?bewit=query-token");

        var middleware = new BewitTokenExtractionMiddleware(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        context.Items["bewitToken"].Should().Be("header-token");
    }

    [Fact]
    public async Task InvokeAsync_WithNoToken_ShouldNotSetContextItem()
    {
        var context = new DefaultHttpContext();

        var middleware = new BewitTokenExtractionMiddleware(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        context.Items.Should().NotContainKey("bewitToken");
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyHeader_ShouldFallbackToQueryParam()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["bewitToken"] = "";
        context.Request.QueryString = new QueryString("?bewit=fallback-token");

        var middleware = new BewitTokenExtractionMiddleware(
            _ => Task.CompletedTask,
            CreateOptions());

        await middleware.InvokeAsync(context);

        context.Items["bewitToken"].Should().Be("fallback-token");
    }

    [Fact]
    public async Task InvokeAsync_WithCustomHeaderName_ShouldReadFromCustomHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-My-Token"] = "custom-header-token";

        var middleware = new BewitTokenExtractionMiddleware(
            _ => Task.CompletedTask,
            CreateOptions(o => o.HeaderName = "X-My-Token"));

        await middleware.InvokeAsync(context);

        context.Items["X-My-Token"].Should().Be("custom-header-token");
    }

    [Fact]
    public async Task InvokeAsync_WithCustomQueryParamName_ShouldReadFromCustomParam()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?token=custom-query-token");

        var middleware = new BewitTokenExtractionMiddleware(
            _ => Task.CompletedTask,
            CreateOptions(o => o.QueryParamName = "token"));

        await middleware.InvokeAsync(context);

        context.Items["bewitToken"].Should().Be("custom-query-token");
    }

    [Fact]
    public async Task InvokeAsync_ShouldAlwaysCallNext()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;

        var middleware = new BewitTokenExtractionMiddleware(
            _ =>
            {
                nextCalled = true;

                return Task.CompletedTask;
            },
            CreateOptions());

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
