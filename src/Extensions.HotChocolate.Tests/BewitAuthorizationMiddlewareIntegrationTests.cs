using System;
using System.Threading.Tasks;
using Bewit.Generation;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace Bewit.HotChocolate.Tests
{
    public class BewitAuthorizationMiddlewareIntegrationTests
        : IClassFixture<MongoResource>
    {
        private readonly MongoResource _mongoResource;

        public BewitAuthorizationMiddlewareIntegrationTests(MongoResource mongoResource)
        {
            _mongoResource = mongoResource;
        }

        [Fact]
        public async Task Query_WhenAuthorize_Success()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider(_mongoResource);
            var payload = new CustomPayload {Email = "foo@bar.gmail.com"};
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            var schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            var result = await TestHelpers.ExecuteQuery(schema, token);

            // assert
            var bewitContext = serviceProvider.GetService<IBewitContext>();
            var customPayload = await bewitContext.GetAsync<CustomPayload>();
            new { QueryResult = result, BewitContext = customPayload }.MatchSnapshot();
        }
    }
}