namespace Bewit.Validation;

/// <summary>
/// Observes expired Bewit tokens after their integrity has been validated.
/// Implementations must be thread-safe because they are consumed by a singleton validator.
/// </summary>
/// <typeparam name="T">The token payload type.</typeparam>
public interface IBewitTokenValidationObserver<T> where T : notnull
{
    /// <summary>
    /// Called immediately before <see cref="Bewit.Exceptions.BewitExpiredException"/> is thrown.
    /// </summary>
    ValueTask OnTokenExpiredAsync(
        BewitTokenExpiredContext<T> context,
        CancellationToken cancellationToken);
}
