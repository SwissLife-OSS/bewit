namespace Bewit.Compatibility.V8;

public sealed class BewitV8CompatibilityOptions
{
    public string Secret { get; set; } = string.Empty;
    public BewitExpirationMode ExpirationMode { get; set; } = BewitExpirationMode.SelfContained;
    public string? PayloadTypeName { get; set; }
    public DateTimeOffset? AcceptUntil { get; set; }
    public int MaximumTokenSizeBytes { get; set; } = 16 * 1024;
}

internal sealed record BewitV8CompatibilityOptions<TPayload>(
    BewitV8CompatibilityOptions Value)
    where TPayload : notnull;
