using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB
{
    internal class MongoNonceRepository : INonceRepository
    {
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

        public MongoNonceRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<Token>(collectionName);
        }

        public async ValueTask InsertOneAsync(
            Token token, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(token, cancellationToken: cancellationToken);
        }

        public async ValueTask<Token> TakeOneAsync(
            string token, CancellationToken cancellationToken)
        {
            FilterDefinition<Token> findFilter = Builders<Token>.Filter.Eq(n => n.Nonce, token);

            return await _collection
                .FindOneAndDeleteAsync(findFilter, cancellationToken: cancellationToken);
        }

        public static void Initialize()
        {
            //ensure static constructor is called
        }
    }
}
