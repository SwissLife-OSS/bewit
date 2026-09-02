namespace Bewit.Validation;

/// <summary>
/// Contains trusted token information when Bewit rejects an expired token.
/// The payload has already passed integrity validation before this context is created.
/// </summary>
/// <typeparam name="T">The token payload type.</typeparam>
public sealed record BewitTokenExpiredContext<T>(
    T Payload,
    Guid Nonce,
    DateTime ExpirationDate,
    DateTime ValidatedAt,
    ExpiryMode ExpiryMode)
    where T : notnull;
