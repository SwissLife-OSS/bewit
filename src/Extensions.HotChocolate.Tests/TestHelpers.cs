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
            ISchema schema, string token = null)
        {
            IQueryRequestBuilder requestBuilder = QueryRequestBuilder.New()
                .SetQuery(@"{ foo }");

            if (token != null)
            {
                requestBuilder.AddProperty(BewitTokenHeader.Value, token);
            }

            return await schema.MakeExecutable()
                .ExecuteAsync(requestBuilder
                    .Create());
        }

        public static ISchema CreateSchema(
            IServiceProvider serviceProvider)
        {
            return SchemaBuilder.New()
                .AddBewitAuthorizeDirectiveType()
                .AddQueryType(c =>
                    c.Name("Query")
                        .Field("foo")
                        .Type<StringType>()
                        .Resolver("bar")
                        .AuthorizeBewit())
                .AddServices(serviceProvider)
                .Create();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "secret"),
                    new KeyValuePair<string, string>("Bewit:TokenDuration", "0:00:05:00")
                })
                .Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddBewitAuthorization(configuration)
                .AddBewitGeneration<object>(configuration)
                .BuildServiceProvider();

            return serviceProvider;
        }

        public static IServiceProvider CreateServiceProvider(MongoResource mongoResource)
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

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddBewitAuthorization(configuration, builder =>
                    builder.UseMongoPersistance(configuration))
                .AddBewitGeneration<object>(configuration, builder => builder.UseMongoPersistance(configuration))
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
