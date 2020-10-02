using System.Collections.Generic;
using Bewit.Core;
using Bewit.Generation;
using Bewit.HotChocolate;
using Bewit.MongoDB;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Configuration;
using HotChocolate.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Bewit.IntegrationTests.HotChocolateServer
{
    internal static class HCServerHelper
    {
        internal static TestServer CreateHotChcocolateServer(
            string secret,
            string connectionString,
            string databaseName
        )
        {
            ISchema schema = SchemaBuilder.New()
                .SetOptions(new SchemaOptions
                {
                    StrictValidation = false
                })
                .AddMutationType(
                    new ObjectType(
                        d =>
                        {
                            d.Name("Mutation");
                            d.Field("RequestAccessToken")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx => "foo")
                                .UseBewitProtection<string>();
                            d.Field("RequestAccessUrlWithQueryString")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx => "http://foo.bar/api/dummy/WithBewitProtection?foo=bar&baz=qux")
                                .UseBewitUrlProtection();
                            d.Field("RequestAccessUrl")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx => "http://foo.bar/api/dummy/WithBewitProtection")
                                .UseBewitUrlProtection();
                        }))
                .Create();

            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    //for url protection
                    services.AddBewitGeneration<string>(
                        new BewitOptions
                        {
                            Secret = secret
                        },
                        builder => builder
                            .UseHmacSha256Encryption()
                            .UseMongoPersistance(
                                new BewitMongoOptions
                                {
                                    ConnectionString = connectionString,
                                    DatabaseName = databaseName
                                })
                    );

                    //for payload protection
                    services.AddBewitGeneration<IDictionary<string, object>>(
                        new BewitOptions
                        {
                            Secret = secret
                        },
                        builder => builder
                            .UseHmacSha256Encryption()
                            .UseMongoPersistance(
                                new BewitMongoOptions
                                {
                                    ConnectionString = connectionString,
                                    DatabaseName = databaseName
                                })
                    );

                    services.AddGraphQL(schema);
                })
                .Configure(app =>
                    app.UseGraphQL());

            return new TestServer(hostBuilder);
        }
    }
}
