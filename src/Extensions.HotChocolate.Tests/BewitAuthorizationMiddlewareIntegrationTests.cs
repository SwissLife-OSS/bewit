using System;
using System.Threading.Tasks;
using HotChocolate;
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
            IServiceProvider serviceProvider = TestHelpers.CreateServiceProvider(_mongoResource);
            var payload = new CustomPayload { Email = "foo@bar.gmail.com" };
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            ISchema schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(schema, token);

            // assert
            IBewitContext bewitContext = serviceProvider.GetService<IBewitContext>();
            CustomPayload customPayload = await bewitContext.GetAsync<CustomPayload>();
            new { QueryResult = result, BewitContext = customPayload }.MatchSnapshot();
        }
    }
}
