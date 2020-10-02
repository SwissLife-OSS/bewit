using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bewit.IntegrationTests.HotChocolateServer;
using Bewit.IntegrationTests.MvcServer;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.IntegrationTests
{
    public class HotChocolateWithMvcTests: IClassFixture<MongoResource>
    {
        public class GiveMeAccessResult
        {
            public string RequestAccess { get; set; }
        }

        private readonly MongoResource _mongoResource;

        public HotChocolateWithMvcTests(MongoResource mongoResource)
        {
            _mongoResource = mongoResource;
        }

        [Fact]
        public async Task GenerateBewitForUrlOnHotChocolate_ValidateOnMvc_ShouldAccept()
        {
            //Arrange MVC Server
            string secret = "4r8FfT!$p0Ortz";
            IMongoDatabase database = _mongoResource.CreateDatabase();
            TestServer hcServer = HCServerHelper.CreateHotChcocolateServer(
                secret,
                _mongoResource.ConnectionString,
                database.DatabaseNamespace.DatabaseName);
            HttpClient hcClient = hcServer.CreateClient();
            GraphQLClient gqlHcClient = new GraphQLClient(hcClient);
            QueryRequest query = new QueryRequest(
                string.Empty,
                @"mutation giveMeAccess {
                    RequestAccess: RequestAccessUrl
                }",
                "giveMeAccess",
                new Dictionary<string, object>());

            TestServer mvcServer = MvcServerHelper.CreateMvcServer(
                secret,
                _mongoResource.ConnectionString,
                database.DatabaseNamespace.DatabaseName);
            HttpClient mvcClient = mvcServer.CreateClient();

            //Act
            /* 1. Get Url from HotChcolate*/
            QueryResponse<GiveMeAccessResult> requireAccessResult =
                await gqlHcClient.QueryAsync<GiveMeAccessResult>(query,
                    CancellationToken.None);
            /* 2. Invoke this url on the MvcServer */
            HttpResponseMessage mvcResponse = await mvcClient.GetAsync(
                requireAccessResult.Data.RequestAccess,
                CancellationToken.None);

            //Assert
            mvcResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await mvcResponse.Content.ReadAsStringAsync();
            content.Should().Be("bar");
        }

        [Fact]
        public async Task GenerateBewitForUrlOnHotChocolate_ValidateOnMvcWithQueryString_ShouldAccept()
        {
            //Arrange MVC Server
            string secret = "4r8FfT!$p0Ortz";
            IMongoDatabase database = _mongoResource.CreateDatabase();
            TestServer hcServer = HCServerHelper.CreateHotChcocolateServer(
                secret,
                _mongoResource.ConnectionString,
                database.DatabaseNamespace.DatabaseName);
            HttpClient hcClient = hcServer.CreateClient();
            GraphQLClient gqlHcClient = new GraphQLClient(hcClient);
            QueryRequest query = new QueryRequest(
                string.Empty,
                @"mutation giveMeAccess {
                    RequestAccess: RequestAccessUrlWithQueryString
                }",
                "giveMeAccess",
                new Dictionary<string, object>());

            TestServer mvcServer = MvcServerHelper.CreateMvcServer(
                secret,
                _mongoResource.ConnectionString,
                database.DatabaseNamespace.DatabaseName);
            HttpClient mvcClient = mvcServer.CreateClient();

            //Act
            /* 1. Get Url from HotChcolate*/
            QueryResponse<GiveMeAccessResult> requireAccessResult =
                await gqlHcClient.QueryAsync<GiveMeAccessResult>(query,
                    CancellationToken.None);
            /* 2. Invoke this url on the MvcServer */
            HttpResponseMessage mvcResponse = await mvcClient.GetAsync(
                requireAccessResult.Data.RequestAccess,
                CancellationToken.None);

            //Assert
            mvcResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            string content = await mvcResponse.Content.ReadAsStringAsync();
            content.Should().Be("bar");
        }
    }
}
