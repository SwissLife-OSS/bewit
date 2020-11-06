using System;
using System.Threading.Tasks;
using HotChocolate;
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
            IServiceProvider services = TestHelpers.CreateSchema();
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(services, payload);
            await TestHelpers.ExecuteQuery(services, token);

            // act
            var context = await services.GetService<IBewitContext>().GetAsync();

            // assert
            Assert.Equal(payload, context);
        }

        [Fact]
        public async Task BewitContext_GetObjectPayload_Success()
        {
            // arrange
            IServiceProvider services = TestHelpers.CreateSchema();
            var payloadContent = "foo@bar.gmail.com";
            var payload = new CustomPayload { Email = payloadContent };
            var token = await TestHelpers.CreateToken(services, payload);
            await TestHelpers.ExecuteQuery(services, token);

            // act
            CustomPayload context
                = await services.GetService<IBewitContext>().GetAsync<CustomPayload>();

            // assert
            Assert.NotNull(context);
            Assert.True(context.Email == payloadContent);
        }

        [Fact]
        public async Task BewitContext_GetWrongTypePayload_Fail()
        {
            // arrange
            IServiceProvider services = TestHelpers.CreateSchema();
            // Refactor: Create here a CustomPayload when BewitContext is done right.
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(services, payload);
            await TestHelpers.ExecuteQuery(services, token);

            // act, assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await services.GetService<IBewitContext>().GetAsync<WrongPayload>());
        }
    }

    internal class CustomPayload
    {
        public string Email { get; set; }
    }

    internal class WrongPayload { }
}
