using System;
using System.Collections.Generic;
using System.Text.Json;
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
                cm.MapField(c => c.IsDeleted);
                cm.MapField(c => c.ExtraProperties);
                cm.SetIgnoreExtraElements(true);
            });
        }

        public MongoNonceRepository(IMongoDatabase database, MongoNonceOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _collection = database.GetCollection<Token>(options.CollectionName);

            _collection.Indexes.CreateOne(new CreateIndexModel<Token>(
                Builders<Token>.IndexKeys.Ascending(nameof(IdentifiableToken.Identifier))));

            _collection.Indexes.CreateOne(new CreateIndexModel<Token>(
                Builders<Token>.IndexKeys.Ascending(nameof(Token.ExpirationDate)),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(options.RecordExpireAfterDays)
                }));
        }

        public async ValueTask InsertOneAsync(
            Token token, CancellationToken cancellationToken)
        {
            token.ExtraProperties = token.ExtraProperties ?? new Dictionary<string, object>();

            IReadOnlySet<string> propertyNamesWithPrimitiveValueType =
                GetPropertyNamesWithPrimitiveValueType(token.ExtraProperties);

            SetJsonStringValue(token.ExtraProperties, propertyNamesWithPrimitiveValueType);

            await _collection.InsertOneAsync(token, cancellationToken: cancellationToken);

            CreateIndexes(propertyNamesWithPrimitiveValueType);
        }

        public async ValueTask<Token?> TakeOneAsync(
            string token,
            CancellationToken cancellationToken)
        {
            FilterDefinition<Token> findFilter =
                Builders<Token>.Filter.Eq(n => n.Nonce, token) &
                (Builders<Token>.Filter.Not(Builders<Token>.Filter.Exists(n => n.IsDeleted)) |
                 Builders<Token>.Filter.Eq(n => n.IsDeleted, false));

            UpdateDefinition<Token> updateDefinition =
                Builders<Token>.Update.Set(x => x.IsDeleted, true);

            if (_options.NonceUsage == NonceUsage.OneTime)
            {
                return await _collection
                    .FindOneAndUpdateAsync(findFilter, updateDefinition, cancellationToken: cancellationToken);
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

        private void CreateIndexes(IEnumerable<string> extraPropertyNames)
        {
            var indexOptions = new CreateIndexOptions { Background = true };

            foreach (string extraPropertyName in extraPropertyNames)
            {
                _collection.Indexes.CreateOne(new CreateIndexModel<Token>(
                    Builders<Token>.IndexKeys.Ascending(
                        $"{nameof(Token.ExtraProperties)}.{extraPropertyName}"), indexOptions));
            }
        }

        private IReadOnlySet<string> GetPropertyNamesWithPrimitiveValueType(
            Dictionary<string, object> extraProperties)
        {
            HashSet<string> names = new HashSet<string>();

            foreach (KeyValuePair<string, object> keyValue in extraProperties)
            {
                if (keyValue.Value == null)
                {
                    continue;
                }

                if (TypeChecker.IsPrimitiveType(keyValue.Value.GetType()))
                {
                    names.Add(keyValue.Key);
                }
            }

            return names;
        }

        private void SetJsonStringValue(
            Dictionary<string, object> extraProperties, IReadOnlySet<string> namesToSkip)
        {
            foreach(string name in extraProperties.Keys)
            {
                if (namesToSkip.Contains(name))
                {
                    continue;
                }

                extraProperties[name] = JsonSerializer.Serialize(extraProperties[name]);
            }
        }
    }
}
