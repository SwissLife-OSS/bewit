namespace Bewit;

public interface IBewitTokenGenerator<TPayload> where TPayload : notnull
{
    ValueTask<BewitToken<TPayload>> GenerateAsync(
        TPayload payload,
        BewitTokenOptions? options = null,
        CancellationToken cancellationToken = default);
}
