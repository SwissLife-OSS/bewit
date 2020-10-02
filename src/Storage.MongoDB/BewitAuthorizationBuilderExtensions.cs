using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB
{
    public static class BewitAuthorizationBuilderExtensions
    {
        public static BewitRegistrationBuilder UseMongoPersistance(
            this BewitRegistrationBuilder builder,
            IConfiguration configuration)
        {
            BewitMongoOptions options =
                configuration.GetSection("Bewit:Mongo").Get<BewitMongoOptions>();

            return builder.UseMongoPersistance(options);
        }

        public static BewitRegistrationBuilder UseMongoPersistance(
            this BewitRegistrationBuilder builder,
            BewitMongoOptions options)
        {
            options.Validate();

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var client =
                new MongoClient(options.ConnectionString);
            IMongoDatabase db =
                client.GetDatabase(options.DatabaseName);

            builder.GetRepository = () => new NonceRepository(
                db, options.CollectionName ?? nameof(Token));

            return builder;
        }
    }
}
