using System.Collections.Generic;
using Bewit.Mvc.Filter;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Extensions.Http.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBewitEndpointAuthorization_WithConfiguration__ShouldAddBewitTokenValidatorForString()
        {
            //Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "123")
                })
                .Build();

            //Act
            services.AddBewitEndpointAuthorization(configuration);

            //Assert
            ServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = services.BuildServiceProvider();
                IBewitTokenValidator<Foo> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenValidator<Foo>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<BewitTokenValidator<Foo>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }
    }
}
