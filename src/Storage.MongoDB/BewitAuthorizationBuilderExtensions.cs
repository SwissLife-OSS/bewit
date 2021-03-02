using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace Bewit.Storage.MongoDB
{
    public static class BewitAuthorizationBuilderExtensions
    {
        public static void UseMongoPersistence(
            this BewitPayload builder,
            IConfiguration configuration)
        {
            MongoNonceOptions options = configuration
                .GetSection("Bewit:Mongo")
                .Get<MongoNonceOptions>();

            builder.UseMongoPersistence(options);
        }

        public static void UseMongoPersistence(
            this BewitPayload builder,
            MongoNonceOptions options)
        {
            options.Validate();

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton(options);
            builder.Services.TryAddSingleton<IMongoDatabase>(sp =>
            {
                var client = new MongoClient(options.ConnectionString);
                return client.GetDatabase(options.DatabaseName);
            });
            builder.Services.TryAddSingleton<INonceRepository, MongoNonceRepository>();
        }
    }
}
