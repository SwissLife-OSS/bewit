using Microsoft.Extensions.Options;

namespace Bewit;

public sealed class BewitTokenConfiguration
{
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromMinutes(1);
    public BewitExpirationMode ExpirationMode { get; set; } = BewitExpirationMode.SelfContained;
    public BewitTokenUsage Usage { get; set; } = BewitTokenUsage.Reusable;
}

internal sealed class BewitTokenConfigurationValidator(string purpose)
    : IValidateOptions<BewitTokenConfiguration>
{
    public ValidateOptionsResult Validate(string? name, BewitTokenConfiguration options)
    {
        if (!string.Equals(name, purpose, StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Skip;
        }

        if (options.Lifetime <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"Token lifetime for purpose '{purpose}' must be positive.");
        }

        if (options.ExpirationMode == BewitExpirationMode.SelfContained
            && options.Usage == BewitTokenUsage.SingleUse)
        {
            return ValidateOptionsResult.Fail(
                $"Self-contained purpose '{purpose}' cannot be single-use.");
        }

        return ValidateOptionsResult.Success;
    }
}
