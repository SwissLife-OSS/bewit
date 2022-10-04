using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Extensions.HotChocolate.Generation;
using Bewit.Generation;
using FluentAssertions;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Extensions.HotChocolate.Tests.Integration
{
    public class BewitUrlMiddlewareTests
    {
        private class MockedVariablesProvider : IVariablesProvider
        {
            public DateTime UtcNow =>
                new DateTime(2017, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            public Guid NextToken =>
                new Guid("724e7acc-be57-49a1-8195-46a03c6271c6");
        }

        public class GiveMeAccessResult
        {
            public string RequestAccess { get; set; }
        }

        [Fact]
        public async Task InvokeAsync_WithFixedDateTime_ShouldAlwaysSendSameToken()
        {
            //Arrange
            TestServer testServer = CreateTestServer<string>();
            HttpClient client = testServer.CreateClient();
            GraphQLClient gqlClient = new GraphQLClient(client);
            QueryRequest query = new QueryRequest(
                string.Empty,
                @"mutation giveMeAccess {
                    RequestAccess
                }",
                "giveMeAccess",
                new Dictionary<string, object>());

            //Act
            QueryResponse<GiveMeAccessResult> res =
                await gqlClient.QueryAsync<GiveMeAccessResult>(query,
                    CancellationToken.None);

            //Assert
            res.Data.Should().NotBeNull();
            res.Data.RequestAccess.Should().Be("https://www.google.com/a/b/?c=d&bewit=eyJUb2tlbiI6eyJOb25jZSI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNiIsIkV4cGlyYXRpb25EYXRlIjoiMjAxNy0wMS0wMVQwMTowMjowMS4wMDFaIn0sIlBheWxvYWQiOiIvYS9iLz9jPWQiLCJIYXNoIjoiOWxaak9tTlFqVG0xbUlUVjZ2LzNtU1NBTFBXRndmNXNaQ3pqc3J6eXhwQT0ifQ%253D%253D");
        }

        private static TestServer CreateTestServer<TPayload>()
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();

                    services
                        .AddBewitGeneration(new BewitOptions { Secret = "123" }, b =>
                        {
                            b.AddPayload<string>()
                                .SetVariablesProvider(() => new MockedVariablesProvider());
                        })
                        .AddGraphQLServer()
                        .SetOptions(new SchemaOptions { StrictValidation = false })
                        .AddMutationType(d =>
                        {
                            d.Name("Mutation");
                            d.Field("RequestAccess")
                                .Type<NonNullType<StringType>>()
                                .Resolve(ctx => "https://www.google.com/a/b/?c=d")
                                .UseBewitUrlProtection();
                        });
                })
                .Configure(app => app
                    .UseRouting()
                    .UseEndpoints(endpoint => endpoint.MapGraphQL("/")));

            var testServer = new TestServer(hostBuilder);
            return testServer;
        }
    }
}
