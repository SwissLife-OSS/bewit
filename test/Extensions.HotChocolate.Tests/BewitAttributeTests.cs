using FluentAssertions;
using Xunit;

namespace Bewit.Extensions.HotChocolate.Tests;

public class BewitAttributeTests
{
    [Fact]
    public void BewitAttribute_ShouldBeApplicableToMethods()
    {
        var attr = typeof(BewitAttribute<string>)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .OfType<AttributeUsageAttribute>()
            .Single();

        attr.ValidOn.Should().HaveFlag(AttributeTargets.Method);
    }

    [Fact]
    public void BewitAttribute_DefaultValues_ShouldBeCorrect()
    {
        var attr = new BewitAttribute<string>();

        attr.SuppressExceptions.Should().BeFalse();
        attr.ExceptionType.Should().BeNull();
    }
}
