using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB
{
    public class NonceRepository : INonceRepository
    {
        private readonly IMongoCollection<Token> _collection;

        static NonceRepository()
        {
            ConventionRegistry.Register(
                "bewit.conventions",
                new ConventionPack
                {
                    new GenericDiscriminatorClassMapConvention()
                }, t => t.FullName?.StartsWith("Bewit") ?? false);

            BsonClassMap.RegisterClassMap<Token>(cm =>
            {
                cm.MapIdMember(c => c.Nonce);
                cm.MapField(c => c.ExpirationDate);
                cm.SetIgnoreExtraElements(true);
            });
        }

        public NonceRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<Token>(collectionName);
        }

        public async Task InsertOneAsync(
            Token token, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(token, new InsertOneOptions(),
                cancellationToken);
        }

        public async Task<Token> FindOneAndDeleteAsync(
            string token, CancellationToken cancellationToken)
        {
            FilterDefinition<Token> findFilter =
                Builders<Token>.Filter.Eq(n => n.Nonce, token);

            return await _collection.FindOneAndDeleteAsync(findFilter,
                new FindOneAndDeleteOptions<Token>(), cancellationToken);
        }

        public static void Initialize()
        {
            //ensure static constructor is called
        }
    }
}
