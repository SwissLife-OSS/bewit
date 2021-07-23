using System.Collections.Generic;
using Bewit.Extensions.HotChocolate.Generation;
using Bewit.Generation;
using Bewit.Storage.MongoDB;
using HotChocolate.Configuration;
using HotChocolate.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.IntegrationTests.HotChocolateServer
{
    internal static class HCServerHelper
    {
        internal static TestServer CreateHotChocolateServer(
            string secret,
            string connectionString,
            string databaseName)
        {

            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();

                    //for url protection
                    services.AddBewitGeneration(
                        new BewitOptions
                        {
                            Secret = secret
                        },
                        builder => builder
                            .AddPayload<string>()
                            .UseMongoPersistence(
                                new MongoNonceOptions
                                {
                                    ConnectionString = connectionString,
                                    DatabaseName = databaseName
                                })
                    );

                    //for payload protection
                    services.AddBewitGeneration(
                        new BewitOptions
                        {
                            Secret = secret
                        },
                        builder => builder
                            .AddPayload<IDictionary<string, object>>()
                            .UseMongoPersistence(
                                new MongoNonceOptions
                                {
                                    ConnectionString = connectionString,
                                    DatabaseName = databaseName
                                })
                    );

                    services.AddGraphQLServer()
                        .SetOptions(new SchemaOptions { StrictValidation = false })
                        .AddMutationType(d =>
                        {
                            d.Name("Mutation");
                            d.Field("RequestAccessToken")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx => "foo")
                                .UseBewitProtection<string>();
                            d.Field("RequestAccessUrlWithQueryString")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx =>
                                    "http://foo.bar/api/dummy/WithBewitProtection?foo=bar&baz=qux")
                                .UseBewitUrlProtection();
                            d.Field("RequestAccessUrl")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx =>
                                    "http://foo.bar/api/dummy/WithBewitProtection")
                                .UseBewitUrlProtection();
                        });
                })
                .Configure(app =>
                    app
                        .UseRouting()
                        .UseEndpoints(endpoint => endpoint.MapGraphQL("/")));

            return new TestServer(hostBuilder);
        }
    }
}
