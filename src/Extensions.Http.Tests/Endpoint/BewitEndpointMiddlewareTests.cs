using Xunit;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using Bewit.Http.Endpoint;
using Bewit.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace SwissLife.ESignature.GraphQL.Tests
{
    public class BewitEndpointMiddlewareTests
    {
        [Fact]
        public async Task BewitEndpointMiddlewareInvokeTest_Without_BewitEndpointAttribute_StatusCodeShouldBe_Ok()
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
        public async Task BewitEndpointMiddlewareInvokeTest_With_BewitEndpointAttribute_NoBewitTokenInUrl_StatusCodeShouldBe_Forbidden()
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
