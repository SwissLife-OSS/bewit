using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Generation;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Bewit.Extensions.Mvc.Tests.Integration
{
    public class BewitAttributeTests
    {
        private const string Secret = "ef56s$e4fs6ef1";

        [Fact]
        public async Task OnAuthorization_WithValidBewitForUrl_ShouldAuthorize()
        {
            //Arrange
            TestServer server = TestServerHelper.CreateServer<IDictionary<string, object>>(Secret);
            var tokenGenerator =
                new BewitTokenGenerator<IDictionary<string, object>>(
                    TimeSpan.FromMinutes(1),
                    new HmacSha256CryptographyService(Secret),
                    new TestServerHelper.MockedVariablesProvider());
            const string id = "1",
                firstName = "John",
                lastName = "Smith";
            var payload =
                new Dictionary<string, object>
                {
                    ["firstName"] = "John",
                    ["lastName"] = "Smith"
                };
            BewitToken<IDictionary<string, object>> bewitToken =
                await tokenGenerator.GenerateBewitTokenAsync(
                    payload,
                    CancellationToken.None);
            var url = $"/api/dummy/WithBewitParameters/{id}";
            var fullUrl = $"{url}?bewit={bewitToken}";
            HttpClient client = server.CreateClient();

            //Act
            HttpResponseMessage res =
                await client.GetAsync(fullUrl, CancellationToken.None);

            //Assert
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await res.Content.ReadAsStringAsync();
            content.Should().Be($"{id}: {firstName} {lastName}");
        }
    }
}
