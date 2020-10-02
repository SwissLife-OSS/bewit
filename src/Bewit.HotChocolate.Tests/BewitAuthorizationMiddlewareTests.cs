using System.Threading.Tasks;
using Snapshooter.Xunit;
using Xunit;

namespace Bewit.HotChocolate.Tests
{
    public class BewitAuthorizationMiddlewareTests
    {
        [Fact]
        public async Task Query_WhenAuthorize_Success()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            var schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            var result = await TestHelpers.ExecuteQuery(schema, token);

            // assert
            result.MatchSnapshot();
        }

        [Fact]
        public async Task Query_WhenWrongToken_Fail()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var token = await TestHelpers.CreateBadToken();
            var schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            var result = await TestHelpers.ExecuteQuery(schema, token);

            // assert
            result.MatchSnapshot(options =>
                options.IgnoreField("Errors[0].Exception.StackTraceString"));
        }

        [Fact]
        public async Task Query_WhenNotAuthorize_Fail()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var schema = TestHelpers.CreateSchema(serviceProvider);

            // act
            var result = await TestHelpers.ExecuteQuery(schema);

            // assert
            result.MatchSnapshot();
        }
    }
}
