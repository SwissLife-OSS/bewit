using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bewit.HotChocolate.Tests
{
    public class BewitContextTests
    {
        [Fact]
        public async Task BewitContext_GetStringPayload_Success()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            var schema = TestHelpers.CreateSchema(serviceProvider);
            await TestHelpers.ExecuteQuery(schema, token);

            // act
            var context = await serviceProvider.GetService<IBewitContext>().GetAsync();

            // assert
            Assert.Equal(payload, context);
        }

        [Fact]
        public async Task BewitContext_GetObjectPayload_Success()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider();
            var payloadContent = "foo@bar.gmail.com";
            var payload = new CustomPayload { Email = payloadContent };
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            var schema = TestHelpers.CreateSchema(serviceProvider);
            await TestHelpers.ExecuteQuery(schema, token);

            // act
            var context = await serviceProvider.GetService<IBewitContext>().GetAsync<CustomPayload>();

            // assert
            Assert.NotNull(context);
            Assert.True(context.Email == payloadContent);
        }

        [Fact]
        public async Task BewitContext_GetWrongTypePayload_Fail()
        {
            // arrange
            var serviceProvider = TestHelpers.CreateServiceProvider();
            // Refactor: Create here a CustomPayload when BewitContext is done right.
            var payload = "foo@bar.gmail.com";
            var token = await TestHelpers.CreateToken(serviceProvider, payload);
            var schema = TestHelpers.CreateSchema(serviceProvider);
            await TestHelpers.ExecuteQuery(schema, token);

            // act, assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await serviceProvider.GetService<IBewitContext>().GetAsync<WrongPayload>());
        }
    }

    internal class CustomPayload
    {
        public string Email { get; set; }
    }

    internal class WrongPayload { }
}