using Microsoft.Extensions.Options;

namespace Bewit;

public sealed class BewitOptions
{
    public string CurrentKeyId { get; set; } = string.Empty;

    public Dictionary<string, string> SigningKeys { get; set; } = [];

    public int MaximumTokenSizeBytes { get; set; } = 16 * 1024;
}

internal sealed class BewitOptionsValidator : IValidateOptions<BewitOptions>
{
    public ValidateOptionsResult Validate(string? name, BewitOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CurrentKeyId))
        {
            return ValidateOptionsResult.Fail("CurrentKeyId is required.");
        }

        if (!options.SigningKeys.TryGetValue(options.CurrentKeyId, out _))
        {
            return ValidateOptionsResult.Fail(
                $"Signing key '{options.CurrentKeyId}' is not configured.");
        }

        if (options.SigningKeys.Any(pair =>
                string.IsNullOrWhiteSpace(pair.Key)
                || System.Text.Encoding.UTF8.GetByteCount(pair.Value) < 32))
        {
            return ValidateOptionsResult.Fail(
                "Every signing key must have a non-empty ID and contain at least 32 UTF-8 bytes.");
        }

        if (options.MaximumTokenSizeBytes is < 256 or > 1024 * 1024)
        {
            return ValidateOptionsResult.Fail(
                "MaximumTokenSizeBytes must be between 256 and 1048576.");
        }

        return ValidateOptionsResult.Success;
    }
}
