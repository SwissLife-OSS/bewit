namespace Bewit.Generation;

public interface IBewitTokenGenerator<T> where T : notnull
{
    ValueTask<BewitToken<T>> GenerateBewitTokenAsync(
        T payload,
        CancellationToken cancellationToken) =>
        GenerateBewitTokenAsync(payload, null, cancellationToken);

    ValueTask<BewitToken<T>> GenerateBewitTokenAsync(
        T payload,
        BewitTokenOptions? options,
        CancellationToken cancellationToken);
}
