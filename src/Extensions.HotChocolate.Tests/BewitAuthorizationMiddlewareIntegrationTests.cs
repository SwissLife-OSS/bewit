using System;
using System.Threading.Tasks;
using Bewit.Extensions.HotChocolate.Validation;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Snapshooter.Xunit;
using Squadron;
using Xunit;

namespace Bewit.Extensions.HotChocolate.Tests
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
            IServiceProvider services = TestHelpers.CreateSchema<CustomPayload>();
            var payload = new CustomPayload { Email = "foo@bar.gmail.com" };
            var token = await TestHelpers.CreateToken(services, payload);

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(services, token);

            // assert
            IBewitContext bewitContext = services.GetService<IBewitContext>();
            CustomPayload customPayload = await bewitContext.GetAsync<CustomPayload>();
            new { QueryResult = result, BewitContext = customPayload }.MatchSnapshot();
        }
    }
}
