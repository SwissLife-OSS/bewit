using FluentAssertions;
using Xunit;

namespace Bewit.Extensions.Mvc.Tests;

public class FromBewitAttributeTests
{
    [Fact]
    public void FromBewitAttribute_ShouldBeApplicableToParameters()
    {
        var attr = typeof(FromBewitAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .OfType<AttributeUsageAttribute>()
            .Single();

        attr.ValidOn.Should().HaveFlag(AttributeTargets.Parameter);
    }
}

public class BewitUrlAuthorizationAttributeTests
{
    [Fact]
    public void BewitUrlAuthorizationAttribute_ShouldBeApplicableToClassesAndMethods()
    {
        var attr = typeof(BewitUrlAuthorizationAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .OfType<AttributeUsageAttribute>()
            .Single();

        attr.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        attr.ValidOn.Should().HaveFlag(AttributeTargets.Method);
    }
}
