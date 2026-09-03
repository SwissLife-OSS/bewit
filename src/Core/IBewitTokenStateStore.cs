namespace Bewit;

/// <summary>
/// Storage-provider extension point. Applications normally consume
/// <see cref="IBewitTokenRepository{TPayload}"/> instead.
/// </summary>
public interface IBewitTokenStateStore
{
    BewitTokenFormat Format { get; }

    bool SupportsPurpose(string purpose);

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

    ValueTask<long> UpdateExpirationByIdentifierAsync(
        string purpose,
        string identifier,
        DateTimeOffset expiration,
        CancellationToken cancellationToken);

    ValueTask<long> RevokeByIdentifierAsync(
        string purpose,
        string identifier,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken);
}
