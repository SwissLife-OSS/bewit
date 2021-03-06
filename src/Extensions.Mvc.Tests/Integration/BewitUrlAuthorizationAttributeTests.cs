using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Generation;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Bewit.Extensions.Mvc.Tests.Integration
{
    public class BewitUrlAuthorizationAttributeTests
    {
        private const string Secret = "456";

        [Fact]
        public async Task SampleGetRequest_NoBewitProtectionOnRoute_ShouldPass()
        {
            //Arrange
            TestServer server = TestServerHelper.CreateServer<string>(Secret);
            var url = "/api/dummy/NoBewitProtection";
            HttpClient client = server.CreateClient();

            //Act
            HttpResponseMessage res =
                await client.GetAsync(url, CancellationToken.None);

            //Assert
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await res.Content.ReadAsStringAsync();
            content.Should().Be("bar");
        }

        [Fact]
        public async Task OnAuthorization_WithValidBewitForUrl_ShouldAuthorize()
        {
            //Arrange
            var cryptoService = new HmacSha256CryptographyService(Secret);
            TestServer server = TestServerHelper.CreateServer<string>(Secret);
            var url = "/api/dummy/WithBewitProtection";
            var tokenGenerator =
                new BewitTokenGenerator<string>(
                    TimeSpan.FromMinutes(1),
                    cryptoService,
                    new TestServerHelper.MockedVariablesProvider());
            BewitToken<string> bewitToken =
                await tokenGenerator.GenerateBewitTokenAsync(
                    url.ToLowerInvariant(),
                    CancellationToken.None);
            var fullUrl = $"{url}?bewit={bewitToken}";
            HttpClient client = server.CreateClient();

            //Act
            HttpResponseMessage res =
                await client.GetAsync(fullUrl, CancellationToken.None);

            //Assert
            res.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await res.Content.ReadAsStringAsync();
            content.Should().Be("bar");
        }

        /// <summary>
        /// In this test we try to access a bewit protected url with a
        /// bewit generated for a different url.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task OnAuthorization_WithDifferentUrl_ShouldNotAuthorize()
        {
            //Arrange
            var cryptoService = new HmacSha256CryptographyService(Secret);
            TestServer server = TestServerHelper.CreateServer<string>(Secret);
            var url = "/api/dummy/SomeBewitProtectedUrl";
            var tokenGenerator =
                new BewitTokenGenerator<string>(
                    TimeSpan.FromMinutes(1),
                    cryptoService,
                    new TestServerHelper.MockedVariablesProvider());
            BewitToken<string> bewitToken =
                await tokenGenerator.GenerateBewitTokenAsync(url.ToLowerInvariant(),
                    CancellationToken.None);
            url = "/api/dummy/WithBewitProtection";
            var fullUrl = $"{url}?bewit={bewitToken}";
            HttpClient client = server.CreateClient();

            //Act
            HttpResponseMessage res =
                await client.GetAsync(fullUrl, CancellationToken.None);

            //Assert
            res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var content = await res.Content.ReadAsStringAsync();
            if (content != null)
            {
                Assert.Equal(-1, content.IndexOf("bar"));
            }
        }

        [Fact]
        public async Task OnAuthorization_WithAlteredPayloadForUrl_ShouldNotAuthorize()
        {
            //Arrange
            var cryptoService = new HmacSha256CryptographyService(Secret);
            TestServer server = TestServerHelper.CreateServer<string>(Secret);
            var url = "/api/dummy/SomeBewitProtectedUrl";
            var tokenGenerator =
                new BewitTokenGenerator<string>(
                    TimeSpan.FromMinutes(1),
                    cryptoService,
                    new TestServerHelper.MockedVariablesProvider());
            BewitToken<string> bewitToken =
                await tokenGenerator.GenerateBewitTokenAsync(url.ToLowerInvariant(),
                    CancellationToken.None);

            //try to hack the token by replacing the url but reusing the same hash
            url = "/api/dummy/WithBewitProtection";
            var serializedBewit =
                Encoding.UTF8.GetString(Convert.FromBase64String((string)bewitToken));
            Bewit<string> bewitInternal =
                JsonConvert.DeserializeObject<Bewit<string>>(serializedBewit);
            var newBewitInternal = new Bewit<string>(
                bewitInternal.Nonce,
                bewitInternal.ExpirationDate,
                url.ToLowerInvariant(),
                bewitInternal.Hash);
            serializedBewit = JsonConvert.SerializeObject(newBewitInternal);
            bewitToken = new BewitToken<string>(Convert.ToBase64String(
                Encoding.UTF8.GetBytes(serializedBewit)
            ));

            var fullUrl = $"{url}?bewit={bewitToken}";
            HttpClient client = server.CreateClient();

            //Act
            HttpResponseMessage res =
                await client.GetAsync(fullUrl, CancellationToken.None);

            //Assert
            res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var content = await res.Content.ReadAsStringAsync();
            if (content != null)
            {
                Assert.Equal(-1, content.IndexOf("bar"));
            }
        }
    }
}
