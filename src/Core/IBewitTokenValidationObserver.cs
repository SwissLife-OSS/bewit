namespace Bewit;

public interface IBewitTokenValidationObserver<TPayload> where TPayload : notnull
{
    ValueTask OnValidatedAsync(
        BewitTokenValidationContext<TPayload> context,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    ValueTask OnExpiredAsync(
        BewitTokenValidationContext<TPayload> context,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;

    ValueTask OnRejectedAsync(
        BewitTokenRejectedContext context,
        CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
