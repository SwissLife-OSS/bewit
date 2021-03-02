using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB
{
    public static class BewitAuthorizationBuilderExtensions
    {
        public static void UseMongoPersistence(
            this BewitPayloadBuilder builder,
            IConfiguration configuration)
        {
            BewitMongoOptions options =
                configuration.GetSection("Bewit:Mongo").Get<BewitMongoOptions>();

            builder.UseMongoPersistence(options);
        }

        public static void UseMongoPersistence(
            this BewitPayloadBuilder builder,
            BewitMongoOptions options)
        {
            options.Validate();

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var client = new MongoClient(options.ConnectionString);
            IMongoDatabase db = client.GetDatabase(options.DatabaseName);

            builder.SetRepository(() => new NonceRepository(
                db, options.CollectionName ?? nameof(Token)));
        }
    }
}
