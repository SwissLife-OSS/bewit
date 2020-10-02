using Bewit.Validation.Exceptions;
using FluentAssertions;
using Xunit;

namespace Bewit.Validation.Tests.Exceptions
{
    public class BewitInvalidExceptionTests
    {
        [Fact]
        public void Constructor_NoParams_ShouldReturnInstance()
        {
            //Act
            var ex = new BewitInvalidException();

            //Assert
            ex.Message.Should().Be("The given Bewit is invalid.");
        }
    }
}
