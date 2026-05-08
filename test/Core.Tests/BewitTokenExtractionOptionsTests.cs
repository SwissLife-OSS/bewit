using FluentAssertions;
using Xunit;

namespace Bewit.Tests;

public class BewitTokenExtractionOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var options = new BewitTokenExtractionOptions();

        options.HeaderName.Should().Be("bewitToken");
        options.QueryParamName.Should().Be("bewit");
    }

    [Fact]
    public void HeaderName_ShouldBeSettable()
    {
        var options = new BewitTokenExtractionOptions
        {
            HeaderName = "X-Custom-Token"
        };

        options.HeaderName.Should().Be("X-Custom-Token");
    }

    [Fact]
    public void QueryParamName_ShouldBeSettable()
    {
        var options = new BewitTokenExtractionOptions
        {
            QueryParamName = "token"
        };

        options.QueryParamName.Should().Be("token");
    }
}
