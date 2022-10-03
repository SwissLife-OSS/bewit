using System;
using System.Threading.Tasks;
using Bewit.Http.Endpoint;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Bewit.Extensions.Http.Tests
{
    public class EndpointConventionBuilderExtensionsTests
    {
        [Fact]
        public void AddBewitEndpointAuthorization_WithConfiguration_ShouldAddBewitTokenValidatorForString()
        {
            //Arrange
            var conventionBuilder = new MockEndpointConventionBuilder();

            //Act
            conventionBuilder.RequireBewitUrlAuthorization();

            //Assert
            conventionBuilder.EndpointBuilder.Metadata.Should().Contain(s =>
                s.GetType() == typeof(BewitEndpointAttribute));
        }
    }

    public class MockEndpointConventionBuilder : IEndpointConventionBuilder
    {
        public MockEndpointBuilder EndpointBuilder = new MockEndpointBuilder();

        public void Add(Action<EndpointBuilder> convention)
        {
            convention.Invoke(EndpointBuilder);
        }
    }

    public class MockEndpointBuilder : EndpointBuilder
    {
        public override Endpoint Build()
        {
            return new Endpoint(
                c => Task.CompletedTask,
                new EndpointMetadataCollection(),
                "foo");
        }
    }
}
