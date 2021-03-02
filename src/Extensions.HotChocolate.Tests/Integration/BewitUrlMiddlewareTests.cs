using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Extensions.HotChocolate.Generation;
using Bewit.Generation;
using FluentAssertions;
using HotChocolate;
using HotChocolate.Configuration;
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
            res.Data.RequestAccess.Should().Be("https://www.google.com/a/b/?c=d&bewit=eyJQYXlsb2FkIjoiL2EvYi8%252FYz1kIiwiSGFzaCI6IjlsWmpPbU5RalRtMW1JVFY2di8zbVNTQUxQV0Z3ZjVzWkN6anNyenl4cEE9IiwiTm9uY2UiOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzYiLCJFeHBpcmF0aW9uRGF0ZSI6IjIwMTctMDEtMDFUMDE6MDI6MDEuMDAxWiJ9");
        }

        private static TestServer CreateTestServer<TPayload>()
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();

                    services
                        .AddSingleton<IVariablesProvider, MockedVariablesProvider>()
                        .AddBewitGeneration<string>(new BewitOptions { Secret = "123" })
                        .AddGraphQLServer()
                        .SetOptions(new SchemaOptions { StrictValidation = false })
                        .AddMutationType(d =>
                        {
                            d.Name("Mutation");
                            d.Field("RequestAccess")
                                .Type<NonNullType<StringType>>()
                                .Resolver(ctx => "https://www.google.com/a/b/?c=d")
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
