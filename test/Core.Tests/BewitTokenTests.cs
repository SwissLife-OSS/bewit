using FluentAssertions;
using Xunit;

namespace Bewit.Tests;

public class BewitTokenTests
{
    [Fact]
    public void Constructor_ShouldStoreValue()
    {
        var token = new BewitToken<string>("abc123");

        ((string)token).Should().Be("abc123");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var a = new BewitToken<string>("abc");
        var b = new BewitToken<string>("abc");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentValue_ShouldNotBeEqual()
    {
        var a = new BewitToken<string>("abc");
        var b = new BewitToken<string>("xyz");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var token = new BewitToken<string>("abc123");

        token.ToString().Should().Be("abc123");
    }
}
