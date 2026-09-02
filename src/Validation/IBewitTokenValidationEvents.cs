namespace Bewit.Validation;

/// <summary>
/// Handles lifecycle events raised while a Bewit token is being validated.
/// Implementations must be thread-safe because they are consumed by a singleton validator.
/// </summary>
/// <typeparam name="T">The token payload type.</typeparam>
public interface IBewitTokenValidationEvents<T> where T : notnull
{
    /// <summary>
    /// Called after token integrity has been verified and before expiry or nonce validation.
    /// </summary>
    ValueTask OnValidatingAsync(
        BewitTokenValidatingContext<T> context,
        CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    /// <summary>
    /// Called immediately before <see cref="Bewit.Exceptions.BewitExpiredException"/> is thrown.
    /// </summary>
    ValueTask OnExpiredAsync(
        BewitTokenExpiredContext<T> context,
        CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;
}
