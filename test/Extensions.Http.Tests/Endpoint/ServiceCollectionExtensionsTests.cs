using System.Collections.Generic;
using Bewit.Http.Endpoint;
using Bewit.Validation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Extensions.Http.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBewitEndpointAuthorization_WithConfiguration_ShouldAddBewitTokenValidatorForString()
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
                IBewitTokenValidator<string> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenValidator<string>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<BewitTokenValidator<string>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }
    }
}
