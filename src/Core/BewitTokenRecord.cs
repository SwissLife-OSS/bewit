using System.Text.Json;

namespace Bewit;

public sealed record BewitTokenRecord(
    BewitTokenReference Reference,
    DateTimeOffset ExpiresAt,
    BewitTokenUsage Usage,
    BewitTokenStatus Status,
    string? Identifier,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConsumedAt = null,
    DateTimeOffset? RevokedAt = null,
    IReadOnlyDictionary<string, JsonElement>? Metadata = null);
