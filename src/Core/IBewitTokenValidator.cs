namespace Bewit;

public interface IBewitTokenValidator<TPayload> where TPayload : notnull
{
    ValueTask<TPayload> ValidateAsync(
        BewitToken<TPayload> token,
        CancellationToken cancellationToken = default);
}
