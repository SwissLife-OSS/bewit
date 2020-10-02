using Bewit.Validation.Exceptions;
using FluentAssertions;
using Xunit;

namespace Bewit.Validation.Tests.Exceptions
{
    public class BewitNotFoundExceptionTests
    {
        [Fact]
        public void Constructor_NoParams_ShouldReturnInstance()
        {
            //Act
            var ex = new BewitNotFoundException();

            //Assert
            ex.Message.Should().Be("The given Bewit was not found.");
        }
    }
}
