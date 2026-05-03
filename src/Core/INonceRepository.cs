namespace Bewit;

/// <summary>
/// Stores and retrieves nonce records for stateful token validation.
/// Implementations provide persistence (e.g., MongoDB) while the default
/// implementation is a no-op for stateless <see cref="ExpiryMode.SelfContained"/> usage.
/// </summary>
/// <example>
/// <code>
/// // Insert a nonce on token generation
/// await repository.InsertOneAsync(token, cancellationToken);
///
/// // Validate and consume a nonce
/// var nonce = await repository.TakeOneAsync(nonceId, cancellationToken);
///
/// // Extend expiry for sliding window
/// await repository.ExtendExpiryAsync(nonceId, TimeSpan.FromMinutes(30), cancellationToken);
///
/// // Admin extends share link expiry
/// await repository.UpdateExpiryAsync(nonceId, newExpiresAt, cancellationToken);
///
/// // Revoke all tokens for an identifier
/// await repository.DeleteIdentifierAsync("user-123", cancellationToken);
/// </code>
/// </example>
public interface INonceRepository
{
    /// <summary>
    /// Stores a nonce record. Called during token generation for stateful tokens.
    /// </summary>
    ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a nonce record by its nonce ID.
    /// For <see cref="NonceUsage.OneTime"/>, marks the record as deleted atomically.
    /// For <see cref="NonceUsage.ReUse"/>, returns the record without modification.
    /// Returns <c>null</c> if the nonce is not found or already consumed.
    /// </summary>
    ValueTask<Token?> TakeOneAsync(Guid nonce, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all nonce records matching the given identifier.
    /// Used for bulk revocation (e.g., revoke all tokens for a user or share link).
    /// </summary>
    ValueTask DeleteIdentifierAsync(string identifier, CancellationToken cancellationToken);

    /// <summary>
    /// Extends the expiry of a nonce record by the specified duration from now.
    /// Used for sliding window session extension.
    /// Returns <c>true</c> if the record was found and updated, <c>false</c> otherwise.
    /// </summary>
    ValueTask<bool> ExtendExpiryAsync(
        Guid nonce,
        TimeSpan duration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets the expiry of a nonce record to an absolute date.
    /// Used when an admin extends or shortens a share link's lifetime.
    /// Returns <c>true</c> if the record was found and updated, <c>false</c> otherwise.
    /// </summary>
    ValueTask<bool> UpdateExpiryAsync(
        Guid nonce,
        DateTime newExpiry,
        CancellationToken cancellationToken);
}
