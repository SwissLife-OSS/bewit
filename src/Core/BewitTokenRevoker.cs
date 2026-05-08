namespace Bewit;

internal sealed class BewitTokenRevoker<T>(
    INonceRepository nonceRepository) : IBewitTokenRevoker<T>
    where T : notnull
{
    public ValueTask RevokeByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken) =>
        nonceRepository.DeleteIdentifierAsync(identifier, cancellationToken);
}
