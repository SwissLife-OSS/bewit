using Microsoft.Extensions.Options;

namespace Bewit;

internal sealed class BewitNonceRequirementValidator(
    string name,
    bool hasRealNonceRepo) : IValidateOptions<BewitOptions>
{
    public ValidateOptionsResult Validate(string? optionsName, BewitOptions options)
    {
        if (optionsName != name)
        {
            return ValidateOptionsResult.Skip;
        }

        if (options.ExpiryMode == ExpiryMode.ServerControlled && !hasRealNonceRepo)
        {
            return ValidateOptionsResult.Fail(
                $"Payload '{name}' uses ExpiryMode.ServerControlled but no persistent " +
                "nonce repository is configured. Call UseMongoDb() or UseNonceRepository() " +
                "on the BewitBuilder or PayloadBuilder.");
        }

        if (options.SlidingWindow.HasValue && !hasRealNonceRepo)
        {
            return ValidateOptionsResult.Fail(
                $"Payload '{name}' has SlidingWindow configured but no persistent " +
                "nonce repository is configured. Call UseMongoDb() or UseNonceRepository() " +
                "on the BewitBuilder or PayloadBuilder.");
        }

        if (options.SlidingWindow.HasValue && options.ExpiryMode != ExpiryMode.ServerControlled)
        {
            return ValidateOptionsResult.Fail(
                $"Payload '{name}' has SlidingWindow configured but ExpiryMode is not " +
                "ServerControlled. SlidingWindow requires ExpiryMode.ServerControlled.");
        }

        return ValidateOptionsResult.Success;
    }
}
