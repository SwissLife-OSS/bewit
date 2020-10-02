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
            IServiceProvider serviceProvider = TestHelpers.CreateServiceProvider();
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            ISchema schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(schema, token);

            // assert
            result.MatchSnapshot();
        }

        [Fact]
        public async Task Query_WhenWrongToken_Fail()
        {
            // arrange
            IServiceProvider serviceProvider = TestHelpers.CreateServiceProvider();
            var token = await TestHelpers.CreateBadToken();
            ISchema schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(schema, token);

            // assert
            result.MatchSnapshot(options =>
                options.IgnoreField("Errors[0].Exception.StackTraceString"));
        }

        [Fact]
        public async Task Query_WhenNotAuthorize_Fail()
        {
            // arrange
            IServiceProvider serviceProvider = TestHelpers.CreateServiceProvider();
            ISchema schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            IExecutionResult result = await TestHelpers.ExecuteQuery(schema);

            // assert
            result.MatchSnapshot();
        }
    }
}
