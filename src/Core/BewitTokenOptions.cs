using System.Text.Json;

namespace Bewit;

public sealed class BewitTokenOptions
{
    public TimeSpan? Lifetime { get; set; }

    public string? Identifier { get; set; }

    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; set; }
}
