namespace Bewit.Validation;

public interface IBewitTokenValidator<T> where T : notnull
{
    ValueTask<T> ValidateBewitTokenAsync(
        BewitToken<T> token,
        CancellationToken cancellationToken);
}
