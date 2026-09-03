using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bewit.MongoDB;

internal sealed class MongoBewitTokenStateStore : IBewitTokenStateStore
{
    private readonly IMongoCollection<BewitTokenDocument> _collection;
    private readonly Lazy<Task> _initialize;

    public MongoBewitTokenStateStore(IMongoDatabase database, IOptions<BewitMongoOptions> options)
    {
        BewitMongoOptions value = options.Value;
        _collection = database.GetCollection<BewitTokenDocument>(value.CollectionName);
        _initialize = new Lazy<Task>(
            () => CreateIndexesAsync(value.RecordExpireAfterDays),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public BewitTokenFormat Format => BewitTokenFormat.V9;
    public bool SupportsPurpose(string purpose) => !string.IsNullOrWhiteSpace(purpose);

    public async ValueTask CreateAsync(BewitTokenRecord record, CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        await _collection.InsertOneAsync(
            BewitTokenDocument.FromRecord(record), cancellationToken: cancellationToken);
    }

    public async ValueTask<BewitTokenRecord?> GetAsync(
        BewitTokenReference reference,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        BewitTokenDocument? document = await _collection.Find(ReferenceFilter(reference))
            .FirstOrDefaultAsync(cancellationToken);
        return document?.ToRecord();
    }

    public async ValueTask<BewitTokenConsumeResult> TryConsumeAsync(
        BewitTokenReference reference,
        DateTimeOffset consumedAt,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        FilterDefinition<BewitTokenDocument> filter = Builders<BewitTokenDocument>.Filter.And(
            ReferenceFilter(reference),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Status, BewitTokenStatus.Active),
            Builders<BewitTokenDocument>.Filter.Gt(x => x.ExpiresAt, consumedAt.UtcDateTime));
        UpdateDefinition<BewitTokenDocument> update = Builders<BewitTokenDocument>.Update
            .Set(x => x.Status, BewitTokenStatus.Consumed)
            .Set(x => x.ConsumedAt, consumedAt.UtcDateTime);
        BewitTokenDocument? consumed = await _collection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<BewitTokenDocument> { ReturnDocument = ReturnDocument.After },
            cancellationToken);
        if (consumed is not null)
        {
            return new BewitTokenConsumeResult(BewitTokenConsumeStatus.Consumed, consumed.ToRecord());
        }

        BewitTokenRecord? current = await GetAsync(reference, cancellationToken);
        BewitTokenConsumeStatus status = current switch
        {
            null => BewitTokenConsumeStatus.NotFound,
            { Status: BewitTokenStatus.Consumed } => BewitTokenConsumeStatus.AlreadyConsumed,
            { Status: BewitTokenStatus.Revoked } => BewitTokenConsumeStatus.Revoked,
            { ExpiresAt: var expiry } when expiry <= consumedAt => BewitTokenConsumeStatus.Expired,
            _ => BewitTokenConsumeStatus.NotFound
        };
        return new BewitTokenConsumeResult(status, current);
    }

    public async ValueTask<bool> UpdateExpirationAsync(
        BewitTokenReference reference,
        DateTimeOffset expiration,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        FilterDefinition<BewitTokenDocument> filter = Builders<BewitTokenDocument>.Filter.And(
            ReferenceFilter(reference),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Status, BewitTokenStatus.Active));
        UpdateResult result = await _collection.UpdateOneAsync(
            filter,
            Builders<BewitTokenDocument>.Update.Set(x => x.ExpiresAt, expiration.UtcDateTime),
            cancellationToken: cancellationToken);
        return result.MatchedCount == 1;
    }

    public async ValueTask<long> UpdateExpirationByIdentifierAsync(
        string purpose,
        string identifier,
        DateTimeOffset expiration,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        FilterDefinition<BewitTokenDocument> filter = Builders<BewitTokenDocument>.Filter.And(
            PurposeIdentifierFilter(purpose, identifier),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Status, BewitTokenStatus.Active));
        UpdateResult result = await _collection.UpdateManyAsync(
            filter,
            Builders<BewitTokenDocument>.Update.Set(x => x.ExpiresAt, expiration.UtcDateTime),
            cancellationToken: cancellationToken);
        return result.MatchedCount;
    }

    public async ValueTask<long> RevokeByIdentifierAsync(
        string purpose,
        string identifier,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);
        FilterDefinition<BewitTokenDocument> filter = Builders<BewitTokenDocument>.Filter.And(
            PurposeIdentifierFilter(purpose, identifier),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Status, BewitTokenStatus.Active));
        UpdateDefinition<BewitTokenDocument> update = Builders<BewitTokenDocument>.Update
            .Set(x => x.Status, BewitTokenStatus.Revoked)
            .Set(x => x.RevokedAt, revokedAt.UtcDateTime);
        UpdateResult result = await _collection.UpdateManyAsync(
            filter, update, cancellationToken: cancellationToken);
        return result.MatchedCount;
    }

    private FilterDefinition<BewitTokenDocument> ReferenceFilter(BewitTokenReference reference) =>
        Builders<BewitTokenDocument>.Filter.And(
            Builders<BewitTokenDocument>.Filter.Eq(x => x.TokenId, reference.TokenId),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Purpose, reference.Purpose),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Format, reference.Format));

    private static FilterDefinition<BewitTokenDocument> PurposeIdentifierFilter(string purpose, string identifier) =>
        Builders<BewitTokenDocument>.Filter.And(
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Purpose, purpose),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Identifier, identifier),
            Builders<BewitTokenDocument>.Filter.Eq(x => x.Format, BewitTokenFormat.V9));

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken) =>
        await _initialize.Value.WaitAsync(cancellationToken);

    private Task CreateIndexesAsync(int recordExpireAfterDays)
    {
        CreateIndexModel<BewitTokenDocument>[] indexes =
        [
            new(
                Builders<BewitTokenDocument>.IndexKeys
                    .Ascending(x => x.Purpose)
                    .Ascending(x => x.Identifier),
                new CreateIndexOptions { Name = "ix_purpose_identifier", Sparse = true }),
            new(
                Builders<BewitTokenDocument>.IndexKeys.Ascending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "ix_ttl", ExpireAfter = TimeSpan.FromDays(recordExpireAfterDays) })
        ];
        return _collection.Indexes.CreateManyAsync(indexes);
    }
}
