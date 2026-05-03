namespace Bewit;

/// <summary>
/// No-op implementation of <see cref="INonceRepository"/> for stateless
/// <see cref="ExpiryMode.SelfContained"/> tokens that don't require persistence.
/// </summary>
internal sealed class DefaultNonceRepository : INonceRepository
{
    public ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask<Token?> TakeOneAsync(Guid nonce, CancellationToken cancellationToken) =>
        new(Token.Empty);

    public ValueTask DeleteIdentifierAsync(string identifier, CancellationToken cancellationToken) =>
        throw new NotSupportedException(
            "Bulk invalidation requires a persistent nonce repository (e.g., MongoDB).");

    public ValueTask<bool> ExtendExpiryAsync(
        Guid nonce,
        TimeSpan duration,
        CancellationToken cancellationToken) =>
        new(false);

    public ValueTask<bool> UpdateExpiryAsync(
        Guid nonce,
        DateTime newExpiry,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException(
            "Expiry updates require a persistent nonce repository (e.g., MongoDB).");
}
