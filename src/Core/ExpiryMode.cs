namespace Bewit;

/// <summary>
/// Controls how token expiration is enforced.
/// </summary>
public enum ExpiryMode
{
    /// <summary>
    /// Expiration is embedded in the token and validated client-side.
    /// The token's <c>ExpirationDate</c> is authoritative.
    /// </summary>
    SelfContained = 0,

    /// <summary>
    /// Expiration is stored in the nonce repository (database) and validated server-side.
    /// The token carries a generous fallback expiry; the database record is authoritative.
    /// Supports admin-driven expiry changes and revocation.
    /// </summary>
    ServerControlled = 1
}
