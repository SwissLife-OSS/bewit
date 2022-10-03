using Xunit;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using Bewit.Http.Endpoint;
using Bewit.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Bewit;
using Bewit.Validation.Exceptions;

namespace SwissLife.ESignature.GraphQL.Tests
{
    public class BewitEndpointMiddlewareTests
    {
        [Fact]
        public async Task BewitEndpointMiddlewareInvokeTest_WithoutBewitEndpointAttribute_StatusCodeShouldBe_Ok()
        {
            // Arrange
            Mock<IBewitTokenValidator<string>> validatorMock =
                new Mock<IBewitTokenValidator<string>>();

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set((new MockEndpointFeature
            {
                Endpoint = new Endpoint(
                    c => Task.CompletedTask,
                    new EndpointMetadataCollection(),
                    "foo")
            }));

            BewitEndpointMiddleware middleware =
                new BewitEndpointMiddleware(c =>
                    Task.CompletedTask,
                    validatorMock.Object);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task BewitEndpointMiddlewareInvokeTest_WithBewitEndpointAttribute_NoBewitTokenInUrl_StatusCodeShouldBe_Forbidden()
        {
            // Arrange
            Mock<IBewitTokenValidator<string>> validatorMock =
                new Mock<IBewitTokenValidator<string>>();

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<IEndpointFeature>(new MockEndpointFeature
            {
                Endpoint = new Endpoint(
                    c => Task.CompletedTask,
                    new EndpointMetadataCollection(new BewitEndpointAttribute()),
                    "foo")
            });

            BewitEndpointMiddleware middleware =
                new BewitEndpointMiddleware(c =>
                    Task.CompletedTask,
                    validatorMock.Object);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task BewitEndpointMiddlewareInvokeTest_WithBewitEndpointAttribute_InValidBewitToken_StatusCodeShouldBe_Forbidden()
        {
            // Arrange
            Mock<IBewitTokenValidator<string>> validatorMock =
                new Mock<IBewitTokenValidator<string>>();
            validatorMock.Setup(p =>
                    p.ValidateBewitTokenAsync(
                        It.IsAny<BewitToken<string>>(),
                        default))
                .Throws<BewitInvalidException>();

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    { "bewit", new StringValues("foo") }
                });
            httpContext.Features.Set<IEndpointFeature>(new MockEndpointFeature
            {
                Endpoint = new Endpoint(
                    c => Task.CompletedTask,
                    new EndpointMetadataCollection(new BewitEndpointAttribute()),
                    "foo")
            });

            BewitEndpointMiddleware middleware =
                new BewitEndpointMiddleware(c =>
                    Task.CompletedTask,
                    validatorMock.Object);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task BewitEndpointMiddlewareInvokeTest_WithBewitEndpointAttribute_ValidBewitToken_PayloadDoesNotMatch_StatusCodeShouldBe_Forbidden()
        {
            // Arrange
            Mock<IBewitTokenValidator<string>> validatorMock =
                new Mock<IBewitTokenValidator<string>>();
            validatorMock.Setup(p =>
                    p.ValidateBewitTokenAsync(
                        It.IsAny<BewitToken<string>>(),
                        default))
                .ReturnsAsync("/path?xyz");

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString("/path?123");
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    { "bewit", new StringValues("foo") }
                });
            httpContext.Features.Set<IEndpointFeature>(new MockEndpointFeature
            {
                Endpoint = new Endpoint(
                    c => Task.CompletedTask,
                    new EndpointMetadataCollection(new BewitEndpointAttribute()),
                    "foo")
            });

            BewitEndpointMiddleware middleware =
                new BewitEndpointMiddleware(c =>
                    Task.CompletedTask,
                    validatorMock.Object);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task BewitEndpointMiddlewareInvokeTest_WithBewitEndpointAttribute_ValidBewitToken_PayloadDoesMatch_StatusCodeShouldBe_Ok()
        {
            // Arrange
            Mock<IBewitTokenValidator<string>> validatorMock =
                new Mock<IBewitTokenValidator<string>>();
            validatorMock.Setup(p =>
                    p.ValidateBewitTokenAsync(
                        It.IsAny<BewitToken<string>>(),
                        default))
                .ReturnsAsync("/path?123");

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString("/path?123");
            httpContext.Request.Query = new QueryCollection(
                new Dictionary<string, StringValues>
                {
                    { "bewit", new StringValues("foo") }
                });
            httpContext.Features.Set<IEndpointFeature>(new MockEndpointFeature
            {
                Endpoint = new Endpoint(
                    c => Task.CompletedTask,
                    new EndpointMetadataCollection(new BewitEndpointAttribute()),
                    "foo")
            });

            BewitEndpointMiddleware middleware =
                new BewitEndpointMiddleware(c =>
                    Task.CompletedTask,
                    validatorMock.Object);

            // Act
            await middleware.Invoke(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }

    public class MockEndpointFeature : IEndpointFeature
    {
        public Endpoint Endpoint
        {
            get;
            set;
        }
    }

}
