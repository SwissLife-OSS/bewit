namespace Bewit;

internal sealed class DefaultNonceRepository : INonceRepository
{
    private const string Message =
        "This operation requires a persistent nonce repository (e.g., MongoDB). " +
        "Configure one via UseMongoDb() or UseNonceRepository() on the BewitBuilder or PayloadBuilder.";

    public ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken) =>
        throw new NotSupportedException(Message);

    public ValueTask<Token?> TakeOneAsync(Guid nonce, CancellationToken cancellationToken) =>
        throw new NotSupportedException(Message);

    public ValueTask DeleteIdentifierAsync(string identifier, CancellationToken cancellationToken) =>
        throw new NotSupportedException(Message);

    public ValueTask<bool> UpdateExpiryAsync(
        Guid nonce,
        DateTime newExpiry,
        CancellationToken cancellationToken) =>
        throw new NotSupportedException(Message);
}
