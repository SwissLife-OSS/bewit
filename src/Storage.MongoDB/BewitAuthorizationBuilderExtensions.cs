using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
#nullable enable

namespace Bewit.Storage.MongoDB
{
    public static class BewitAuthorizationBuilderExtensions
    {
        public static void UseMongoPersistence(
            this BewitPayloadContext context,
            IConfiguration configuration,
            Action<MongoNonceOptions>? configure = default)
        {
            MongoNonceOptions options = configuration
                .GetSection("Bewit:Mongo")
                .Get<MongoNonceOptions>()!;

            configure?.Invoke(options);

            context.UseMongoPersistence(options);
        }

        public static void UseMongoPersistence(
            this BewitPayloadContext context,
            MongoNonceOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Validate(requireConnectionString: true);

            context.SetRepository(() =>
            {
                var client = new MongoClient(GetRequiredConnectionString(options));
                return client.GetRepository(options);
            });
        }

        public static void UseMongoPersistence(
            this BewitPayloadContext context,
            IMongoClient client,
            MongoNonceOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Validate(requireConnectionString: false);

            context.SetRepository(() => client.GetRepository(options));
        }

        private static MongoNonceRepository GetRepository(this IMongoClient client, MongoNonceOptions options)
        {
            IMongoDatabase database = client.GetDatabase(options.DatabaseName);
            return new MongoNonceRepository(database, options);
        }

        private static string GetRequiredConnectionString(MongoNonceOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new ArgumentException(
                    "Value cannot be null or whitespace.",
                    nameof(options.ConnectionString));
            }

            return options.ConnectionString;
        }

    }
}
