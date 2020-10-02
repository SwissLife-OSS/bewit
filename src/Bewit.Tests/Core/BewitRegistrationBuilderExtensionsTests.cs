using Bewit.Core;
using FluentAssertions;
using Xunit;

namespace Bewit.Tests.Core
{
    public class BewitRegistrationBuilderExtensionsTests
    {
        [Fact]
        public void UseHmacSha256Encryption_NoParams_ShouldRegisterHmac265CryptographyService()
        {
            //Arrange
            const string secret = "foo";
            var builder = new BewitRegistrationBuilder();

            //Act
            builder.UseHmacSha256Encryption();

            //Assert
            var options = new BewitOptions
            {
                Secret = secret
            };
            builder.GetCryptographyService(options)
                .GetType().Should().Be(typeof(HmacSha256CryptographyService));
        }
    }
}
