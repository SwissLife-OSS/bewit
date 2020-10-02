using Bewit.Validation.Exceptions;
using FluentAssertions;
using Xunit;

namespace Bewit.Validation.Tests.Exceptions
{
    public class BewitExpiredExceptionTests
    {
        [Fact]
        public void Constructor_NoParams_ShouldReturnInstance()
        {
            //Act
            var ex = new BewitExpiredException();

            //Assert
            ex.Message.Should().Be("The given Bewit has expired.");
        }
    }
}
