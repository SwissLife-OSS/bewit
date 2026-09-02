namespace Bewit.Validation;

/// <summary>
/// Contains trusted token information immediately before state and expiry validation.
/// The payload has already passed integrity validation before this context is created.
/// </summary>
/// <typeparam name="T">The token payload type.</typeparam>
public sealed record BewitTokenValidatingContext<T>(
    T Payload,
    Guid Nonce,
    DateTime ValidatedAt,
    ExpiryMode ExpiryMode,
    DateTime? TokenExpirationDate)
    where T : notnull;
