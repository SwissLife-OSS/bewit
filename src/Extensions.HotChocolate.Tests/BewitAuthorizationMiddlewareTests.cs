using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Snapshooter.Xunit;
using Xunit;

namespace Bewit.Extensions.HotChocolate.Tests
{
    public class BewitAuthorizationMiddlewareTests
    {
        [Fact]
        public async Task Query_WhenAuthorize_Success()
        {
            // arrange
            IServiceProvider serviceProvider = TestHelpers.CreateSchema();
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(serviceProvider, payload);

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(serviceProvider, token);

            // assert
            result.MatchSnapshot();
        }

        [Fact]
        public async Task Query_WhenWrongToken_Fail()
        {
            // arrange
            IServiceProvider serviceProvider = TestHelpers.CreateSchema();
            var token = await TestHelpers.CreateBadToken();

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(serviceProvider, token);

            // assert
            result.MatchSnapshot(options =>
                options.IgnoreField("Errors[0].Exception.StackTraceString"));
        }

        [Fact]
        public async Task Query_WhenNotAuthorize_Fail()
        {
            // arrange
            IServiceProvider serviceProvider = TestHelpers.CreateSchema();

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(serviceProvider);

            // assert
            result.MatchSnapshot();
        }
    }
}
