using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

#nullable enable

namespace Bewit.Tests.Core
{
    public class BewitOptionsTests
    {
        [Fact]
        public void Constructor_OnlyMandatoryParameters_ShouldConstruct()
        {
            //Arrange
            const string secret = "123";

            //Act

            var options = new BewitOptions
            {
                Secret = secret
            };

            //Assert
            options.Should().NotBeNull();
            options.Secret.Should().Be(secret);
            options.TokenDuration.Should().Be(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void FromConfiguration_WithAllKeys_ShouldBindAllKeys()
        {
            //Arrange
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("Secret", "123"),
                    new KeyValuePair<string, string?>("TokenDuration", "2.03:00:00")
                }).Build();

            //Act
            BewitOptions options = config.Get<BewitOptions>()!;

            //Assert
            options.Should().NotBeNull();
            options.Secret.Should().Be("123");
            options.TokenDuration.Should().Be(TimeSpan.FromDays(2)+TimeSpan.FromHours(3));
        }

        [Fact]
        public void Constructor_WithTokenDurationInitialization_ShouldConstruct()
        {
            //Arrange
            const string secret = "123";

            //Act
            var options = new BewitOptions
            {
                Secret = secret,
                TokenDuration = TimeSpan.FromMinutes(2)
            };

            //Assert
            options.Should().NotBeNull();
            options.Secret.Should().Be(secret);
            options.TokenDuration.Should().Be(TimeSpan.FromMinutes(2));
        }
    }
}
