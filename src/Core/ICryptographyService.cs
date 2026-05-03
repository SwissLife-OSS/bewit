namespace Bewit;

/// <summary>
/// Provides cryptographic hash generation for token integrity verification.
/// </summary>
public interface ICryptographyService
{
    /// <summary>
    /// Generates an HMAC hash from the token nonce, expiration date, and payload.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="nonce">The unique token nonce.</param>
    /// <param name="expirationDate">The token expiration date (may be null for server-controlled tokens).</param>
    /// <param name="payload">The application payload.</param>
    /// <returns>A Base64-encoded HMAC-SHA256 hash.</returns>
    string GetHash<T>(Guid nonce, DateTime? expirationDate, T payload) where T : notnull;
}
