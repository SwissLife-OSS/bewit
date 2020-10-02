﻿using System;
using System.Collections.Generic;
using Bewit.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bewit.Validation.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        public class Foo
        {
            public int Bar { get; set; }
        }

        [Fact]
        public void AddBewitValidation_WithValidConfiguration_ShouldAddBewitTokenValidatorForMyPayload()
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
            services.AddBewitValidation<Foo>(configuration);

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

        [Fact]
        public void AddBewitValidation_WithInvalidConfiguration_ShouldAddBewitTokenValidatorForMyPayload()
        {
            //Arrange
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:esfesf", "123")
                })
                .Build();

            //Act
            Action register = () => services.AddBewitValidation<Foo>(configuration);

            //Assert
            register.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddBewitValidation_WithMyPayload_ShouldAddBewitTokenValidatorForMyPayload()
        {
            //Arrange
            const string secret = "112";
            var services = new ServiceCollection();

            //Act
            services.AddBewitValidation<Foo>(new BewitOptions
            {
                Secret = secret
            });

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

        [Fact]
        public void AddBewitValidation_WithPersistance_ShouldAddPersistedBewitTokenGenerator()
        {
            //Arrange
            const string secret = "112";
            var services = new ServiceCollection();

            //Act
            services.AddBewitValidation<Foo>(new BewitOptions
                {
                    Secret = secret
                },
                builder =>
                {
                    builder.GetRepository = () =>
                        new Mock<INonceRepository>().Object;
                });

            //Assert
            ServiceProvider serviceProvider = null;
            try
            {
                serviceProvider = services.BuildServiceProvider();
                IBewitTokenValidator<Foo> bewitTokenGenerator =
                    serviceProvider.GetService<IBewitTokenValidator<Foo>>();
                bewitTokenGenerator.Should().NotBeNull();
                bewitTokenGenerator.Should()
                    .BeOfType<PersistedBewitTokenValidator<Foo>>();
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }

        [Fact]
        public void AddBewitValidation_WithEmptyBuilder_ShouldAddBewitTokenGeneratorForMyPayload()
        {
            //Arrange
            const string secret = "112";
            var services = new ServiceCollection();

            //Act
            services.AddBewitValidation<Foo>(new BewitOptions
                {
                    Secret = secret
                },
                builder => { });

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
