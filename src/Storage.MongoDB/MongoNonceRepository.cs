using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB;

internal sealed class MongoNonceRepository : INonceRepository
{
    private readonly IMongoCollection<NonceDocument> _collection;
    private readonly NonceUsage _nonceUsage;

    public MongoNonceRepository(
        IMongoDatabase database,
        IOptions<BewitMongoOptions> options)
    {
        BewitMongoOptions mongoOptions = options.Value;
        _nonceUsage = mongoOptions.NonceUsage;

        _collection = database.GetCollection<NonceDocument>(mongoOptions.CollectionName);

        EnsureIndexes(mongoOptions.RecordExpireAfterDays);
    }

    public async ValueTask InsertOneAsync(Token token, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(
            NonceDocument.FromToken(token),
            cancellationToken: cancellationToken);
    }

    public async ValueTask<Token?> TakeOneAsync(Guid nonce, CancellationToken cancellationToken)
    {
        FilterDefinition<NonceDocument> filter = Builders<NonceDocument>.Filter.And(
            Builders<NonceDocument>.Filter.Eq(d => d.Nonce, nonce),
            Builders<NonceDocument>.Filter.Eq(d => d.IsDeleted, false));

        if (_nonceUsage == NonceUsage.OneTime)
        {
            UpdateDefinition<NonceDocument> update = Builders<NonceDocument>.Update
                .Set(d => d.IsDeleted, true);

            NonceDocument? doc = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<NonceDocument>
                {
                    ReturnDocument = ReturnDocument.Before
                },
                cancellationToken);

            return doc?.ToToken();
        }

        NonceDocument? found = await _collection
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);

        return found?.ToToken();
    }

    public async ValueTask DeleteIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        FilterDefinition<NonceDocument> filter = Builders<NonceDocument>.Filter
            .Eq(d => d.Identifier, identifier);

        UpdateDefinition<NonceDocument> update = Builders<NonceDocument>.Update
            .Set(d => d.IsDeleted, true);

        await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async ValueTask<bool> UpdateExpiryAsync(
        Guid nonce,
        DateTime newExpiry,
        CancellationToken cancellationToken)
    {
        FilterDefinition<NonceDocument> filter = Builders<NonceDocument>.Filter.And(
            Builders<NonceDocument>.Filter.Eq(d => d.Nonce, nonce),
            Builders<NonceDocument>.Filter.Eq(d => d.IsDeleted, false));

        UpdateDefinition<NonceDocument> update = Builders<NonceDocument>.Update
            .Set(d => d.ExpirationDate, newExpiry);

        UpdateResult result = await _collection.UpdateOneAsync(
            filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    public async ValueTask<bool> UpdateExpiryByIdentifierAsync(
        string identifier,
        DateTime newExpiry,
        CancellationToken cancellationToken)
    {
        FilterDefinition<NonceDocument> filter = Builders<NonceDocument>.Filter.And(
            Builders<NonceDocument>.Filter.Eq(d => d.Identifier, identifier),
            Builders<NonceDocument>.Filter.Eq(d => d.IsDeleted, false));

        UpdateDefinition<NonceDocument> update = Builders<NonceDocument>.Update
            .Set(d => d.ExpirationDate, newExpiry);

        UpdateResult result = await _collection.UpdateManyAsync(
            filter,
            update,
            cancellationToken: cancellationToken);

        return result.MatchedCount > 0;
    }

    private void EnsureIndexes(int expireAfterDays)
    {
        var indexModels = new List<CreateIndexModel<NonceDocument>>
        {
            new(
                Builders<NonceDocument>.IndexKeys.Ascending(d => d.Identifier),
                new CreateIndexOptions { Name = "ix_identifier", Sparse = true }),

            new(
                Builders<NonceDocument>.IndexKeys.Ascending(d => d.CreatedAt),
                new CreateIndexOptions<NonceDocument>
                {
                    Name = "ix_ttl",
                    ExpireAfter = TimeSpan.FromDays(expireAfterDays)
                })
        };

        _collection.Indexes.CreateMany(indexModels);
    }
}
