using System.Collections.Generic;
using Bewit.Core;
using Bewit.Mvc.Filter;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Extensions.Mvc.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddBewitUrlAuthorizationFilter_WithConfiguration_ShouldRegisterBewitUrlAuthorizationFilter()
        {
            //Arrange
            var services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "123"),
                }).Build();

            //Act
            services.AddBewitUrlAuthorizationFilter(
                config, builder => { });

            //Assert
            services.Should().Contain(s =>
                s.ServiceType == typeof(BewitUrlAuthorizationAttribute));
        }

        [Fact]
        public void AddBewitUrlAuthorizationFilter_WithOptions_ShouldRegisterBewitUrlAuthorizationFilter()
        {
            //Arrange
            const string secret = "123";
            var services = new ServiceCollection();

            //Act
            services.AddBewitUrlAuthorizationFilter(
                new BewitOptions
                {
                    Secret = secret
                },
                builder => { });

            //Assert
            services.Should().Contain(s =>
                s.ServiceType == typeof(BewitUrlAuthorizationAttribute));
        }

        [Fact]
        public void AddBewitFilter_WithConfiguration_ShouldRegisterBewitUrlAuthorizationFilter()
        {
            //Arrange
            var services = new ServiceCollection();
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "123"),
                }).Build();

            //Act
            services.AddBewitFilter(
                config, builder => { });

            //Assert
            services.Should().Contain(s =>
                s.ServiceType == typeof(BewitAttribute));
        }

        [Fact]
        public void AddBewitFilter_WithOptions_ShouldRegisterBewitUrlAuthorizationFilter()
        {
            //Arrange
            const string secret = "123";
            var services = new ServiceCollection();

            //Act
            services.AddBewitFilter(
                new BewitOptions
                {
                    Secret = secret
                },
                builder => { });

            //Assert
            services.Should().Contain(s =>
                s.ServiceType == typeof(BewitAttribute));
        }
    }
}
