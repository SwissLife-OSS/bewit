using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.Extensions.HotChocolate.Tests
{
    public class BewitContextTests
    {
        [Fact]
        public async Task BewitContext_GetStringPayload_Success()
        {
            // arrange
            IServiceProvider services = TestHelpers.CreateSchema<string>();
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(services, payload);
            await TestHelpers.ExecuteQuery(services, token);

            // act
            var context = services.GetService<IHttpContextAccessor>().GetBewitPayload<string>();

            // assert
            Assert.Equal(payload, context);
        }

        [Fact]
        public async Task BewitContext_GetObjectPayload_Success()
        {
            // arrange
            IServiceProvider services = TestHelpers.CreateSchema<CustomPayload>();
            var payloadContent = "foo@bar.gmail.com";
            var payload = new CustomPayload { Email = payloadContent };
            var token = await TestHelpers.CreateToken(services, payload);
            await TestHelpers.ExecuteQuery(services, token);

            // act
            CustomPayload context = services.GetService<IHttpContextAccessor>().GetBewitPayload<CustomPayload>();

            // assert
            Assert.NotNull(context);
            Assert.True(context.Email == payloadContent);
        }

        [Fact]
        public async Task BewitContext_GetWrongTypePayload_Fail()
        {
            // arrange
            IServiceProvider services = TestHelpers.CreateSchema<string>();
            // Refactor: Create here a CustomPayload when BewitContext is done right.
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(services, payload);
            await TestHelpers.ExecuteQuery(services, token);

            // act, assert
            Assert.Throws<InvalidOperationException>(() =>
                services.GetService<IHttpContextAccessor>().GetBewitPayload<WrongPayload>());
        }
    }

    internal class CustomPayload
    {
        public string Email { get; set; }
    }

    internal class WrongPayload { }
}
