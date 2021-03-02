using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB
{
    public static class BewitAuthorizationBuilderExtensions
    {
        public static void UseMongoPersistence(
            this BewitPayloadContext context,
            IConfiguration configuration)
        {
            MongoNonceOptions options = configuration
                .GetSection("Bewit:Mongo")
                .Get<MongoNonceOptions>();

            context.UseMongoPersistence(options);
        }

        public static void UseMongoPersistence(
            this BewitPayloadContext context,
            MongoNonceOptions options)
        {
            options.Validate();

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.SetRepository(() =>
            {
                var client = new MongoClient(options.ConnectionString);
                IMongoDatabase database = client.GetDatabase(options.DatabaseName);
                return new MongoNonceRepository(database, options);
            });
        }
    }
}
