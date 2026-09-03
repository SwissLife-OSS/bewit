namespace Bewit;

internal sealed class BewitTokenRepository<TPayload>(
    BewitTokenRegistration<TPayload> registration,
    IEnumerable<IBewitTokenStateStore> stores,
    TimeProvider timeProvider)
    : IBewitTokenRepository<TPayload>
    where TPayload : notnull
{
    private readonly IReadOnlyList<IBewitTokenStateStore> _stores = [.. stores];

    public string Purpose => registration.Purpose;

    public ValueTask CreateAsync(
        BewitTokenRecord record,
        CancellationToken cancellationToken) =>
        Resolve(record.Reference).CreateAsync(record, cancellationToken);

    public ValueTask<BewitTokenRecord?> GetAsync(
        BewitTokenReference reference,
        CancellationToken cancellationToken) =>
        Resolve(reference).GetAsync(reference, cancellationToken);

    public ValueTask<BewitTokenConsumeResult> TryConsumeAsync(
        BewitTokenReference reference,
        DateTimeOffset consumedAt,
        CancellationToken cancellationToken) =>
        Resolve(reference).TryConsumeAsync(reference, consumedAt, cancellationToken);

    public ValueTask<bool> UpdateExpirationAsync(
        BewitTokenReference reference,
        DateTimeOffset expiration,
        CancellationToken cancellationToken) =>
        Resolve(reference).UpdateExpirationAsync(reference, expiration, cancellationToken);

    public async ValueTask<BewitBulkOperationResult> UpdateExpirationByIdentifierAsync(
        string identifier,
        DateTimeOffset expiration,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        var counts = new List<(BewitTokenFormat, long)>();

        foreach (IBewitTokenStateStore store in StoresForPurpose())
        {
            long count = await store.UpdateExpirationByIdentifierAsync(
                Purpose, identifier, expiration, cancellationToken);
            counts.Add((store.Format, count));
        }

        return BewitBulkOperationResult.From(counts);
    }

    public async ValueTask<BewitBulkOperationResult> RevokeByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        DateTimeOffset revokedAt = timeProvider.GetUtcNow();
        var counts = new List<(BewitTokenFormat, long)>();

        foreach (IBewitTokenStateStore store in StoresForPurpose())
        {
            long count = await store.RevokeByIdentifierAsync(
                Purpose, identifier, revokedAt, cancellationToken);
            counts.Add((store.Format, count));
        }

        return BewitBulkOperationResult.From(counts);
    }

    private IEnumerable<IBewitTokenStateStore> StoresForPurpose() =>
        _stores.Where(store => store.SupportsPurpose(Purpose));

    private IBewitTokenStateStore Resolve(BewitTokenReference reference)
    {
        if (!string.Equals(reference.Purpose, Purpose, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Token purpose '{reference.Purpose}' does not match repository purpose '{Purpose}'.");
        }

        return _stores.SingleOrDefault(store =>
                store.Format == reference.Format && store.SupportsPurpose(Purpose))
            ?? throw new InvalidOperationException(
                $"No {reference.Format} token store is configured for purpose '{Purpose}'.");
    }
}
