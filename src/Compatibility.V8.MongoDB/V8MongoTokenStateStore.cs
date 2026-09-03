using MongoDB.Driver;

namespace Bewit.Compatibility.V8.MongoDB;

internal sealed class V8MongoTokenStateStore(
    IMongoDatabase database,
    BewitV8MongoOptions options,
    string purpose)
    : IBewitTokenStateStore
{
    private readonly IMongoCollection<LegacyNonceDocument> _collection =
        database.GetCollection<LegacyNonceDocument>(options.CollectionName);

    public BewitTokenFormat Format => BewitTokenFormat.V8;
    public bool SupportsPurpose(string candidate) => string.Equals(candidate, purpose, StringComparison.Ordinal);

    public ValueTask CreateAsync(BewitTokenRecord record, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The v8 compatibility store is read/update-only and never issues tokens.");

    public async ValueTask<BewitTokenRecord?> GetAsync(
        BewitTokenReference reference,
        CancellationToken cancellationToken)
    {
        LegacyNonceDocument? document = await _collection
            .Find(x => x.Nonce == reference.TokenId)
            .FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToRecord(reference.Purpose, document);
    }

    public async ValueTask<BewitTokenConsumeResult> TryConsumeAsync(
        BewitTokenReference reference,
        DateTimeOffset consumedAt,
        CancellationToken cancellationToken)
    {
        FilterDefinition<LegacyNonceDocument> filter = Builders<LegacyNonceDocument>.Filter.And(
            Builders<LegacyNonceDocument>.Filter.Eq(x => x.Nonce, reference.TokenId),
            Builders<LegacyNonceDocument>.Filter.Eq(x => x.IsDeleted, false),
            Builders<LegacyNonceDocument>.Filter.Gt(x => x.ExpirationDate, consumedAt.UtcDateTime));
        LegacyNonceDocument? consumed = await _collection.FindOneAndUpdateAsync(
            filter,
            Builders<LegacyNonceDocument>.Update.Set(x => x.IsDeleted, true),
            new FindOneAndUpdateOptions<LegacyNonceDocument> { ReturnDocument = ReturnDocument.After },
            cancellationToken);
        if (consumed is not null)
        {
            return new BewitTokenConsumeResult(
                BewitTokenConsumeStatus.Consumed,
                ToRecord(reference.Purpose, consumed) with
                {
                    Status = BewitTokenStatus.Consumed,
                    ConsumedAt = consumedAt
                });
        }

        BewitTokenRecord? current = await GetAsync(reference, cancellationToken);
        BewitTokenConsumeStatus status = current switch
        {
            null => BewitTokenConsumeStatus.NotFound,
            { Status: not BewitTokenStatus.Active } => BewitTokenConsumeStatus.AlreadyConsumed,
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
        UpdateResult result = await _collection.UpdateOneAsync(
            Builders<LegacyNonceDocument>.Filter.And(
                Builders<LegacyNonceDocument>.Filter.Eq(x => x.Nonce, reference.TokenId),
                Builders<LegacyNonceDocument>.Filter.Eq(x => x.IsDeleted, false)),
            Builders<LegacyNonceDocument>.Update.Set(x => x.ExpirationDate, expiration.UtcDateTime),
            cancellationToken: cancellationToken);
        return result.MatchedCount == 1;
    }

    public async ValueTask<long> UpdateExpirationByIdentifierAsync(
        string purpose,
        string identifier,
        DateTimeOffset expiration,
        CancellationToken cancellationToken)
    {
        UpdateResult result = await _collection.UpdateManyAsync(
            Builders<LegacyNonceDocument>.Filter.And(
                Builders<LegacyNonceDocument>.Filter.Eq(x => x.Identifier, identifier),
                Builders<LegacyNonceDocument>.Filter.Eq(x => x.IsDeleted, false)),
            Builders<LegacyNonceDocument>.Update.Set(x => x.ExpirationDate, expiration.UtcDateTime),
            cancellationToken: cancellationToken);
        return result.MatchedCount;
    }

    public async ValueTask<long> RevokeByIdentifierAsync(
        string purpose,
        string identifier,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken)
    {
        UpdateResult result = await _collection.UpdateManyAsync(
            Builders<LegacyNonceDocument>.Filter.And(
                Builders<LegacyNonceDocument>.Filter.Eq(x => x.Identifier, identifier),
                Builders<LegacyNonceDocument>.Filter.Eq(x => x.IsDeleted, false)),
            Builders<LegacyNonceDocument>.Update.Set(x => x.IsDeleted, true),
            cancellationToken: cancellationToken);
        return result.MatchedCount;
    }

    private BewitTokenRecord ToRecord(string purpose, LegacyNonceDocument document) => new(
        new BewitTokenReference(Format, purpose, document.Nonce),
        document.ExpirationDate is null
            ? DateTimeOffset.MinValue
            : new DateTimeOffset(DateTime.SpecifyKind(document.ExpirationDate.Value, DateTimeKind.Utc)),
        options.Usage,
        document.IsDeleted ? BewitTokenStatus.Consumed : BewitTokenStatus.Active,
        document.Identifier,
        new DateTimeOffset(DateTime.SpecifyKind(document.CreatedAt, DateTimeKind.Utc)));
}
