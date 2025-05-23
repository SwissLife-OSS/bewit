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

#nullable enable

namespace Bewit.Extensions.HotChocolate.Tests.Integration
{
    public class BewitMiddlewareTests
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
            public string? RequestAccess { get; set; }
        }

        [Fact]
        public async Task InvokeAsync_WithFixedDateTime_ShouldAlwaysSendSameToken()
        {
            //Arrange
            TestServer testServer = CreateTestServer();
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
            res.Data.RequestAccess.Should().Be("eyJUb2tlbiI6eyJOb25jZSI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNiIsIkV4cGlyYXRpb25EYXRlIjoiMjAxNy0wMS0wMVQwMTowMjowMS4wMDFaIn0sIlBheWxvYWQiOiJmb28iLCJIYXNoIjoiV2Rrclh4ZklZVmZPS0Y2aitVdkdGUitieW5VdDNmakpxRjhMS0tnT2tPaz0ifQ==");
        }

        private static TestServer CreateTestServer()
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();

                    BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                        .SetCryptographyService(() => new HmacSha256CryptographyService(new BewitConfiguration(secret: "123", TimeSpan.FromMinutes(1))))
                        .SetVariablesProvider(() => new MockedVariablesProvider())
                        .SetRepository(() => new DefaultNonceRepository());
                    services.AddTransient<IBewitTokenGenerator<string>>(ctx =>
                        new BewitTokenGenerator<string>(new BewitOptions { Secret = "123" }, context));
                    services
                        .AddGraphQLServer()
                        .ModifyOptions(s => s.StrictValidation = false)
                        .AddMutationType(d =>
                        {
                            d.Name("Mutation");
                            d.Field("RequestAccess")
                                .Type<NonNullType<StringType>>()
                                .Resolve(ctx => "foo")
                                .UseBewitProtection<string>();
                        });
                })
                .Configure(app =>
                    app.UseRouting().UseEndpoints(e => e.MapGraphQL("/")));

            return new TestServer(hostBuilder);
        }
    }
}
