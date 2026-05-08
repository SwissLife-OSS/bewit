namespace Bewit;

public interface IBewitTokenRevoker<T> where T : notnull
{
    ValueTask RevokeByIdentifierAsync(string identifier, CancellationToken cancellationToken);
}
