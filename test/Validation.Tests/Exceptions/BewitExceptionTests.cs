using FluentAssertions;
using Xunit;

namespace Bewit.Validation.Tests.Exceptions
{
    public class BewitExceptionTests
    {
        [Fact]
        public void Constructor_WithErrorMessage_ShouldReturnInstance()
        {
            //Arrange
            const string errorMessage = "foo";
            
            //Act
            var ex = new BewitExceptionAccessor(errorMessage);

            //Assert
            ex.Message.Should().Be(errorMessage);
        }
    }
}
