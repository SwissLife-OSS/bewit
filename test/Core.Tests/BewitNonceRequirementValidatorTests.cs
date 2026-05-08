using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Bewit.Tests;

public class BewitNonceRequirementValidatorTests
{
    private const string PayloadName = "TestPayload";

    [Fact]
    public void Validate_ServerControlled_WithoutNonceRepo_ShouldFail()
    {
        var sut = new BewitNonceRequirementValidator(PayloadName, hasRealNonceRepo: false);
        var options = new BewitOptions
        {
            Secret = "test-secret-at-least-32-chars-long!",
            ExpiryMode = ExpiryMode.ServerControlled
        };

        ValidateOptionsResult result = sut.Validate(PayloadName, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("ServerControlled");
        result.FailureMessage.Should().Contain("UseMongoDb()");
    }

    [Fact]
    public void Validate_ServerControlled_WithNonceRepo_ShouldSucceed()
    {
        var sut = new BewitNonceRequirementValidator(PayloadName, hasRealNonceRepo: true);
        var options = new BewitOptions
        {
            Secret = "test-secret-at-least-32-chars-long!",
            ExpiryMode = ExpiryMode.ServerControlled
        };

        ValidateOptionsResult result = sut.Validate(PayloadName, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_SelfContained_WithoutNonceRepo_ShouldSucceed()
    {
        var sut = new BewitNonceRequirementValidator(PayloadName, hasRealNonceRepo: false);
        var options = new BewitOptions
        {
            Secret = "test-secret-at-least-32-chars-long!",
            ExpiryMode = ExpiryMode.SelfContained
        };

        ValidateOptionsResult result = sut.Validate(PayloadName, options);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_DifferentOptionsName_ShouldSkip()
    {
        var sut = new BewitNonceRequirementValidator(PayloadName, hasRealNonceRepo: false);
        var options = new BewitOptions
        {
            Secret = "test-secret-at-least-32-chars-long!",
            ExpiryMode = ExpiryMode.ServerControlled
        };

        ValidateOptionsResult result = sut.Validate("OtherPayload", options);

        result.Skipped.Should().BeTrue();
    }
}
