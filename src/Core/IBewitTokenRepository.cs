namespace Bewit;

public interface IBewitTokenRepository<TPayload> where TPayload : notnull
{
    string Purpose { get; }

    ValueTask CreateAsync(
        BewitTokenRecord record,
        CancellationToken cancellationToken);

    ValueTask<BewitTokenRecord?> GetAsync(
        BewitTokenReference reference,
        CancellationToken cancellationToken);

    ValueTask<BewitTokenConsumeResult> TryConsumeAsync(
        BewitTokenReference reference,
        DateTimeOffset consumedAt,
        CancellationToken cancellationToken);

    ValueTask<bool> UpdateExpirationAsync(
        BewitTokenReference reference,
        DateTimeOffset expiration,
        CancellationToken cancellationToken);

    ValueTask<BewitBulkOperationResult> UpdateExpirationByIdentifierAsync(
        string identifier,
        DateTimeOffset expiration,
        CancellationToken cancellationToken);

    ValueTask<BewitBulkOperationResult> RevokeByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken);
}
