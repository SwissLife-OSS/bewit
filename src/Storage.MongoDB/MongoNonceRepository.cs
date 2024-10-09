using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

#nullable enable

namespace Bewit.Storage.MongoDB
{
    internal class MongoNonceRepository : INonceRepository
    {
        private readonly MongoNonceOptions _options;
        private readonly IMongoCollection<Token> _collection;

        static MongoNonceRepository()
        {
            ConventionRegistry.Register(
                "bewit.conventions",
                new ConventionPack
                {
                    new DiscriminatorClassMapConvention()
                }, t => t.FullName?.StartsWith("Bewit") ?? false);

            BsonClassMap.RegisterClassMap<Token>(cm =>
            {
                cm.MapIdMember(c => c.Nonce);
                cm.MapField(c => c.ExpirationDate);
                cm.SetIgnoreExtraElements(true);
            });
        }

        public MongoNonceRepository(IMongoDatabase database, MongoNonceOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _collection = database.GetCollection<Token>(options.CollectionName);

            _collection.Indexes.CreateOne(new CreateIndexModel<Token>(
                Builders<Token>.IndexKeys.Ascending(nameof(IdentifiableToken.Identifier))));
        }

        public async ValueTask InsertOneAsync(
            Token token, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(token, cancellationToken: cancellationToken);
        }

        public async ValueTask<Token?> TakeOneAsync(
            string token,
            CancellationToken cancellationToken)
        {
            FilterDefinition<Token> findFilter = Builders<Token>.Filter.Eq(n => n.Nonce, token);

            if (_options.NonceUsage == NonceUsage.OneTime)
            {
                return await _collection
                    .FindOneAndDeleteAsync(findFilter, cancellationToken: cancellationToken);
            }

            return await _collection
                .Find(findFilter)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async ValueTask DeleteIdentifier(
            string identifier,
            CancellationToken cancellationToken)
        {
            FilterDefinition<Token> findFilter = Builders<Token>.Filter
                .Eq(nameof(IdentifiableToken.Identifier), identifier);

            await _collection.DeleteManyAsync(findFilter, cancellationToken);
        }

        public static void Initialize()
        {
            //ensure static constructor is called
        }
    }
}
