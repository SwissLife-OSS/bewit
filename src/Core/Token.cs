using System.Text.Json.Serialization;

namespace Bewit;

/// <summary>
/// Represents a nonce record stored in the repository.
/// Tracks token identity, expiration, deletion state, and optional metadata.
/// </summary>
public sealed class Token
{
    [JsonConstructor]
    public Token(
        Guid nonce,
        DateTime? expirationDate,
        string? identifier,
        bool isDeleted,
        Dictionary<string, object>? extraProperties)
    {
        Nonce = nonce;
        ExpirationDate = expirationDate;
        Identifier = identifier;
        IsDeleted = isDeleted;
        ExtraProperties = extraProperties ?? [];
    }

    /// <summary>
    /// Unique nonce identifying this token instance.
    /// </summary>
    public Guid Nonce { get; }

    /// <summary>
    /// Expiration date of the token.
    /// For <see cref="ExpiryMode.SelfContained"/> this is authoritative.
    /// For <see cref="ExpiryMode.ServerControlled"/> this is a generous fallback;
    /// the nonce repository record holds the real expiry.
    /// Null when expiry is entirely server-controlled and not embedded.
    /// </summary>
    public DateTime? ExpirationDate { get; }

    /// <summary>
    /// Optional identifier for bulk invalidation (e.g., user ID, share link ID).
    /// </summary>
    [JsonIgnore]
    public string? Identifier { get; }

    /// <summary>
    /// Indicates whether this token has been consumed or revoked.
    /// </summary>
    [JsonIgnore]
    public bool IsDeleted { get; internal set; }

    /// <summary>
    /// Additional metadata stored with the nonce record.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, object> ExtraProperties { get; }

    /// <summary>
    /// Creates a new token with the specified nonce and expiration.
    /// </summary>
    public static Token Create(Guid nonce, DateTime? expirationDate, string? identifier = null) =>
        new(nonce, expirationDate, identifier, isDeleted: false, extraProperties: null);

    public static Token Create(
        Guid nonce,
        DateTime? expirationDate,
        string? identifier,
        Dictionary<string, object>? extraProperties) =>
        new(nonce, expirationDate, identifier, isDeleted: false, extraProperties);

    internal static readonly Token Empty = new(Guid.Empty, null, null, false, null);
}
