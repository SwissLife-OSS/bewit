namespace Bewit;

public sealed record BewitBulkOperationResult(
    IReadOnlyDictionary<BewitTokenFormat, long> AffectedByFormat)
{
    public long TotalAffected => AffectedByFormat.Values.Sum();

    public long GetAffectedCount(BewitTokenFormat format) =>
        AffectedByFormat.GetValueOrDefault(format);

    internal static BewitBulkOperationResult From(
        IEnumerable<(BewitTokenFormat Format, long Count)> counts) =>
        new(counts
            .GroupBy(x => x.Format)
            .ToDictionary(group => group.Key, group => group.Sum(x => x.Count)));
}
