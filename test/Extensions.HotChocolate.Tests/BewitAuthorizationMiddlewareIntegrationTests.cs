using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
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
            IHttpContextAccessor httpContextAccessor = services.GetService<IHttpContextAccessor>();
            CustomPayload customPayload = httpContextAccessor.GetBewitPayload<CustomPayload>();
            new { QueryResult = result.ToJson(), BewitContext = customPayload }.MatchSnapshot();
        }
    }
}
