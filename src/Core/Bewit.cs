using System.Text.Json.Serialization;

namespace Bewit;

/// <summary>
/// Internal representation of a complete bewit token, containing the nonce metadata,
/// the application payload, and the HMAC integrity hash.
/// Serialized to JSON and Base64-encoded to produce a <see cref="BewitToken{T}"/>.
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
internal sealed class Bewit<T> where T : notnull
{
    [JsonConstructor]
    public Bewit(Token token, T payload, string hash)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        Hash = !string.IsNullOrWhiteSpace(hash)
            ? hash
            : throw new ArgumentException("Hash must not be empty.", nameof(hash));
    }

    /// <summary>
    /// Nonce metadata (nonce ID, expiration, identifier, etc.).
    /// </summary>
    public Token Token { get; }

    /// <summary>
    /// Application-specific payload carried by this token.
    /// </summary>
    public T Payload { get; }

    /// <summary>
    /// HMAC-SHA256 hash ensuring token integrity.
    /// </summary>
    public string Hash { get; }
}
