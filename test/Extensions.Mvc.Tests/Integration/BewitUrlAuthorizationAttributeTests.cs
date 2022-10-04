using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Generation;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Bewit.Extensions.Mvc.Tests.Integration
{
    public class BewitUrlAuthorizationAttributeTests
    {
        private BewitOptions Options = new BewitOptions {Secret = "456"};

        [Fact]
        public async Task SampleGetRequest_NoBewitProtectionOnRoute_ShouldPass()
        {
            //Arrange
            TestServer server = TestServerHelper.CreateServer<string>(Options);
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
            var cryptoService = new HmacSha256CryptographyService(Options);
            TestServer server = TestServerHelper.CreateServer<string>(Options);
            var url = "/api/dummy/WithBewitProtection";
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => TestServerHelper.VariablesProvider)
                .SetRepository(() => TestServerHelper.NonceRepository);
            var tokenGenerator = new BewitTokenGenerator<string>(Options, context);
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
            var cryptoService = new HmacSha256CryptographyService(Options);
            TestServer server = TestServerHelper.CreateServer<string>(Options);
            var url = "/api/dummy/SomeBewitProtectedUrl";
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => TestServerHelper.VariablesProvider)
                .SetRepository(() => TestServerHelper.NonceRepository);
            var tokenGenerator = new BewitTokenGenerator<string>(Options, context);
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
            var cryptoService = new HmacSha256CryptographyService(Options);
            TestServer server = TestServerHelper.CreateServer<string>(Options);
            var url = "/api/dummy/SomeBewitProtectedUrl";
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => TestServerHelper.VariablesProvider)
                .SetRepository(() => TestServerHelper.NonceRepository);
            var tokenGenerator = new BewitTokenGenerator<string>(Options, context);
            BewitToken<string> bewitToken =
                await tokenGenerator.GenerateBewitTokenAsync(url.ToLowerInvariant(),
                    CancellationToken.None);

            //try to hack the token by replacing the url but reusing the same hash
            url = "/api/dummy/WithBewitProtection";
            var serializedBewit =
                Encoding.UTF8.GetString(Convert.FromBase64String((string)bewitToken));
            Bewit<string> bewitInternal =
                JsonConvert.DeserializeObject<Bewit<string>>(serializedBewit);
            var token = Token.Create(bewitInternal.Token.Nonce, bewitInternal.Token.ExpirationDate);
            var newBewitInternal = new Bewit<string>(
                token,
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
