using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Generation;
using Bewit.Storage.MongoDB;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Squadron;

namespace Bewit.Extensions.HotChocolate.Tests
{
    public static class TestHelpers
    {
        public static async Task<string> CreateToken(
            IServiceProvider serviceProvider, object payload)
        {
            IBewitTokenGenerator<object> bewitGenerator = serviceProvider
                .GetService<IBewitTokenGenerator<object>>();

            return (await bewitGenerator
                    .GenerateBewitTokenAsync(payload, default))
                .ToString();
        }

        public static async Task<string> CreateBadToken()
        {
            var bewitOptions = new BewitOptions
            { Secret = "badSecret", TokenDuration = TimeSpan.FromMinutes(5) };

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddBewitGeneration<object>(bewitOptions)
                .BuildServiceProvider();

            IBewitTokenGenerator<object> bewitGenerator = serviceProvider
                .GetService<IBewitTokenGenerator<object>>();

            return (await bewitGenerator
                    .GenerateBewitTokenAsync("badPayload", default))
                .ToString();
        }

        public static async Task<IExecutionResult> ExecuteQuery(
            IServiceProvider services, string token = null)
        {
            IQueryRequestBuilder requestBuilder =
                QueryRequestBuilder.New()
                    .SetQuery(@"{ foo }");

            if (token != null)
            {
                requestBuilder.AddProperty(BewitTokenHeader.Value, token);
            }

            return await services.ExecuteRequestAsync(requestBuilder.Create());
        }

        public static IServiceProvider CreateSchema()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "secret"),
                    new KeyValuePair<string, string>("Bewit:TokenDuration", "0:00:05:00")
                })

                .Build();
            return new ServiceCollection()
                .AddBewitGeneration<object>(configuration)
                .AddGraphQLServer()
                .AddBewitAuthorizeDirectiveType()
                .AddBewitAuthorization(configuration)
                .AddQueryType(c =>
                    c.Name("Query")
                        .Field("foo")
                        .Type<StringType>()
                        .Resolver("bar")
                        .AuthorizeBewit())
                .Services
                .BuildServiceProvider();
        }


        public static IServiceProvider CreateSchema(MongoResource mongoResource)
        {
            IMongoDatabase db = mongoResource.CreateDatabase();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Bewit:Mongo:ConnectionString", mongoResource.ConnectionString),
                    new KeyValuePair<string, string>("Bewit:Mongo:DatabaseName", db.DatabaseNamespace.DatabaseName),
                    new KeyValuePair<string, string>("Bewit:Mongo:CollectionName", "bewitTokens"),
                    new KeyValuePair<string, string>("Bewit:Secret", "secret"),
                    new KeyValuePair<string, string>("Bewit:TokenDuration", "0:00:05:00")
                })
                .Build();

            return new ServiceCollection()
                .AddBewitGeneration<object>(configuration)
                .AddGraphQLServer()
                .AddBewitAuthorizeDirectiveType()
                .AddBewitAuthorization(configuration)
                .AddQueryType(c =>
                    c.Name("Query")
                        .Field("foo")
                        .Type<StringType>()
                        .Resolver("bar")
                        .AuthorizeBewit())
                .Services
                .AddBewitGeneration<object>(configuration, builder => builder.UseMongoPersistance(configuration))
                .BuildServiceProvider();
        }
    }
}
