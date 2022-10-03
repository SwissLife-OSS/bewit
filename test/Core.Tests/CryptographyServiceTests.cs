using System;
using FluentAssertions;
using Xunit;

namespace Bewit.Tests.Core
{
    public class CryptographyServiceTests
    {
        [Fact]
        public void Constructor_WithSecret_ShouldConstruct()
        {
            //Arrange
            string secret = "esk84j85$85efsf";

            //Act
            var service = new HmacSha256CryptographyService(
                new BewitOptions { Secret = secret });

            //Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithSecretNull_ShouldThrow()
        {
            //Arrange
            string secret = null;

            //Act
            Func<HmacSha256CryptographyService> initService = () =>
                new HmacSha256CryptographyService(
                    new BewitOptions { Secret = secret });

            //Assert
            initService.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WithSecretWhitespacesOnly_ShouldThrow()
        {
            //Arrange
            string secret = " ";

            //Act
            Func<HmacSha256CryptographyService> initService = () =>
                new HmacSha256CryptographyService(
                    new BewitOptions { Secret = secret });

            //Assert
            initService.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetHash_WithAllParamsAndStringPayload_ShouldAlwaysGenerateSameHash()
        {
            //Arrange
            string secret = "esk84j85$85efsf";
            var service = new HmacSha256CryptographyService(
                new BewitOptions { Secret = secret });
            string token = "foo";
            DateTime expirationDate =
                new DateTime(2017, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            string payload = "foo";

            //Act
            string hash = service.GetHash(token, expirationDate, payload);

            //Assert
            hash.Should().Be("Ry+dceBg/qVpCDbw9kByG7HZ769CHiA7NWaQAIa+rg0=");
        }

        [Fact]
        public void GetHash_WithAllParamsAndStringPayload_ShouldAlwaysGenerateSameHash2()
        {
            //Arrange
            string secret = "esk84j85$85efsf";
            var service = new HmacSha256CryptographyService(
                new BewitOptions { Secret = secret });
            string token = "foo";
            DateTime expirationDate =
                new DateTime(2017, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            string payload = "bar";

            //Act
            string hash = service.GetHash(token, expirationDate, payload);

            //Assert
            hash.Should().Be("lBA9v7RxsHC/gSBD50PQaoyWH5XKq8eRZ9KM+Qs6b/g=");
        }
    }
}
