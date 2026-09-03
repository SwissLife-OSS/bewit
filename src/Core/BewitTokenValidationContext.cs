namespace Bewit;

public sealed record BewitTokenValidationContext<TPayload>(
    TPayload Payload,
    BewitTokenReference Reference,
    BewitExpirationMode ExpirationMode,
    DateTimeOffset? Expiration,
    BewitTokenRecord? Record)
    where TPayload : notnull;
