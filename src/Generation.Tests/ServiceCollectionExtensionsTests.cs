using System;
using System.Collections.Generic;
using Bewit.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bewit.Generation.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        public class Foo
        {
            public int Bar { get; set; }
        }

        [Fact]
        public void AddBewitGeneration_WithValidConfiguration_ShouldAddBewitTokenGeneratorForMyPayload()
        {
            //Arrange
            var services = new ServiceCollection();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "123")
                })
                .Build();

            //Act
            services.AddBewitGeneration(configuration, b => b.AddPayload<Foo>());

            //Assert
            ServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = services.BuildServiceProvider();
                IBewitTokenGenerator<Foo> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenGenerator<Foo>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<BewitTokenGenerator<Foo>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }

        [Fact]
        public void AddBewitGeneration_WithInvalidConfiguration_ShouldThrowException()
        {
            //Arrange
            var services = new ServiceCollection();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:esfesf", "123")
                })
                .Build();

            //Act
            Action register = () => services.AddBewitGeneration(configuration, b => b.AddPayload<Foo>());

            //Assert
            register.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddBewitGeneration_WithMyPayload_ShouldAddBewitTokenGeneratorForMyPayload()
        {
            //Arrange
            const string secret = "112";
            var services = new ServiceCollection();

            //Act
            services.AddBewitGeneration(new BewitOptions
            {
                Secret = secret
            }, b => b.AddPayload<Foo>());

            //Assert
            ServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = services.BuildServiceProvider();
                IBewitTokenGenerator<Foo> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenGenerator<Foo>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<BewitTokenGenerator<Foo>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }

        [Fact]
        public void AddBewitGeneration_WithPersistance_ShouldAddPersistedBewitTokenGenerator()
        {
            //Arrange
            const string secret = "112";
            var services = new ServiceCollection();

            //Act
            services.AddSingleton<INonceRepository>(new Mock<INonceRepository>().Object);
            services.AddBewitGeneration(new BewitOptions
                {
                    Secret = secret
                },
                builder =>
                {
                    builder.AddPayload<Foo>();
                });

            //Assert
            ServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = services.BuildServiceProvider();
                IBewitTokenGenerator<Foo> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenGenerator<Foo>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<BewitTokenGenerator<Foo>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }

        [Fact]
        public void AddBewitGeneration_WithEmptyBuilder_ShouldAddBewitTokenGeneratorForMyPayload()
        {
            //Arrange
            const string secret = "112";
            var services = new ServiceCollection();

            //Act
            services.AddBewitGeneration(new BewitOptions
                {
                    Secret = secret
                },
                builder => builder.AddPayload<Foo>());

            //Assert
            ServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = services.BuildServiceProvider();
                IBewitTokenGenerator<Foo> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenGenerator<Foo>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<BewitTokenGenerator<Foo>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }
    }
}
