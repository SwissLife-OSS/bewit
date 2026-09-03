namespace Bewit;

public sealed record BewitDecodedToken<TPayload>(
    BewitTokenReference Reference,
    TPayload Payload,
    BewitExpirationMode ExpirationMode,
    DateTimeOffset? EmbeddedExpiration)
    where TPayload : notnull;
