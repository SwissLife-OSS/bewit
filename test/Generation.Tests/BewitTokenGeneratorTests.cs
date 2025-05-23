using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Validation.Generation.Exceptions;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Bewit.Generation.Tests
{
    public class BewitTokenGeneratorTests
    {
        public class Foo
        {
            public int Bar { get; set; }
        }

        #region Test Constructor

        [Fact]
        public void Constructor_WithAllMandatoryOptions_ShouldInit()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                .SetRepository(() => new DefaultNonceRepository());

            //Act
            var provider = new BewitTokenGenerator<Foo>(new BewitOptions(), context);

            //Assert
            provider.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_MissingCryptoService_ShouldThrow()
        {
            //Arrange
            //Act
            Action initProvider = () =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new BewitTokenGenerator<Foo>(
                    new BewitOptions(), new BewitPayloadContext(typeof(string))
                        .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                        .SetRepository(() => new DefaultNonceRepository()));
            };

            //Assert
            initProvider.Should().Throw<BewitMissingConfigurationException>();
        }

        [Fact]
        public void Constructor_MissingProvider_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();

            //Act
            Action initProvider = () =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new BewitTokenGenerator<Foo>(
                    new BewitOptions(), new BewitPayloadContext(typeof(string))
                        .SetCryptographyService(() => cryptoService)
                        .SetRepository(() => new DefaultNonceRepository()));
            };

            //Assert
            initProvider.Should().Throw<BewitMissingConfigurationException>();
        }

        [Fact]
        public void Constructor_MissingRepository_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();

            //Act
            Action initProvider = () =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new BewitTokenGenerator<Foo>(
                    new BewitOptions(), new BewitPayloadContext(typeof(string))
                        .SetCryptographyService(() => cryptoService)
                        .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider()));
            };

            //Assert
            initProvider.Should().Throw<BewitMissingConfigurationException>();
        }

        #endregion

        #region Test GenerateBewitTokenAsync

        [Fact]
        public async Task GenerateBewitTokenAsync_WithPayload_ShouldGenerateBewit()
        {
            //Arrange
            ICryptographyService cryptoService = MockHelper.GetMockedCrpytoService<Foo>();
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                .SetRepository(() => new DefaultNonceRepository());
            var provider = new BewitTokenGenerator<Foo>(new BewitOptions(), context);
            var payload = new Foo
            {
                Bar = 1
            };

            //Act
            BewitToken<Foo> bewit =
                await provider.GenerateBewitTokenAsync(payload,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            ((string)bewit).Should().Be("eyJUb2tlbiI6eyJOb25jZSI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNiIsIkV4cGlyYXRpb25EYXRlIjoiMjAxNy0wMS0wMVQwMTowMjowMS4wMDFaIn0sIlBheWxvYWQiOnsiQmFyIjoxfSwiSGFzaCI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNl9fMjAxNy0wMS0wMVQwMTowMjowMS4wMDEwMDAwWl9fe1wiQmFyXCI6MX0ifQ==");
        }

        [Fact]
        public async Task GenerateBewitTokenAsync_WithDifferentDateAndRandomToken_ShouldGenerateDifferentBewit()
        {
            //Arrange
            ICryptographyService cryptoService = MockHelper.GetMockedCrpytoService<Foo>();
            BewitPayloadContext context = new BewitPayloadContext(typeof(Foo))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider2())
                .SetRepository(() => new DefaultNonceRepository());
            var provider = new BewitTokenGenerator<Foo>(new BewitOptions(), context);
            var payload = new Foo
            {
                Bar = 1
            };

            //Act
            BewitToken<Foo> bewit =
                await provider.GenerateBewitTokenAsync(payload,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            ((string)bewit).Should().Be("eyJUb2tlbiI6eyJOb25jZSI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNiIsIkV4cGlyYXRpb25EYXRlIjoiMjAxOC0wNi0wNlQwMTowMjowMS4wMDFaIn0sIlBheWxvYWQiOnsiQmFyIjoxfSwiSGFzaCI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNl9fMjAxOC0wNi0wNlQwMTowMjowMS4wMDEwMDAwWl9fe1wiQmFyXCI6MX0ifQ==");
        }

        [Fact]
        public async Task GenerateBewitTokenAsync_WithDifferentPayload_ShouldGenerateBewit()
        {
            //Arrange
            ICryptographyService cryptoService = MockHelper.GetMockedCrpytoService<Foo>();
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                .SetRepository(() => new DefaultNonceRepository());
            var provider = new BewitTokenGenerator<Foo>(new BewitOptions(), context);
            var payload = new Foo
            {
                Bar = 5
            };

            //Act
            BewitToken<Foo> bewit =
                await provider.GenerateBewitTokenAsync(payload,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            ((string)bewit).Should().Be("eyJUb2tlbiI6eyJOb25jZSI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNiIsIkV4cGlyYXRpb25EYXRlIjoiMjAxNy0wMS0wMVQwMTowMjowMS4wMDFaIn0sIlBheWxvYWQiOnsiQmFyIjo1fSwiSGFzaCI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNl9fMjAxNy0wMS0wMVQwMTowMjowMS4wMDEwMDAwWl9fe1wiQmFyXCI6NX0ifQ==");
        }

        [Fact]
        public void GenerateBewitTokenAsync_WithNullPayload_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            BewitPayloadContext context = new BewitPayloadContext(typeof(string))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                .SetRepository(() => new DefaultNonceRepository());
            var provider = new BewitTokenGenerator<Foo>(new BewitOptions(), context);

            //Act
            Func<Task> generateBewit = async () =>
                await provider.GenerateBewitTokenAsync(null,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            generateBewit.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region GenerateBewitAsync

        [Fact]
        public async Task GenerateBewitAsync_WithPayload_ShouldGenerateBewit()
        {
            //Arrange
            var variableProvider = new MockHelper.MockedVariablesProvider();
            var tokenTuration = TimeSpan.FromMinutes(1);
            IBewitTokenGenerator<Foo> provider =
                CreateGenerator<Foo>(
                    tokenTuration,
                    MockHelper.GetMockedCrpytoService<Foo>(),
                    variableProvider);
            var payload = new Foo
            {
                Bar = 1
            };

            //Act
            BewitToken<Foo> token =
                await provider.GenerateBewitTokenAsync(payload,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            Bewit<Foo> bewit = GetBewitFromToken(token);
            bewit.Token.Nonce.Should().Be(variableProvider.NextToken.ToString());
            bewit.Token.ExpirationDate.Should()
                .Be(variableProvider.UtcNow.AddTicks(tokenTuration.Ticks));
            bewit.Payload.Should().BeEquivalentTo(payload);
            bewit.Hash.Should()
                .Be("724e7acc-be57-49a1-8195-46a03c6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":1}");
        }

        [Fact]
        public async Task GenerateBewitAsync_WithDifferentDateAndRandomToken_ShouldGenerateDifferentBewit()
        {
            //Arrange
            var variableProvider = new MockHelper.MockedVariablesProvider2();
            var tokenTuration = TimeSpan.FromMinutes(1);
            IBewitTokenGenerator<Foo> provider =
                CreateGenerator<Foo>(
                    tokenTuration,
                    MockHelper.GetMockedCrpytoService<Foo>(),
                    variableProvider);
            var payload = new Foo
            {
                Bar = 1
            };

            //Act
            BewitToken<Foo> token =
                await provider.GenerateBewitTokenAsync(payload,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            Bewit<Foo> bewit = GetBewitFromToken(token);
            bewit.Token.Nonce.Should().Be(variableProvider.NextToken.ToString());
            bewit.Token.ExpirationDate.Should()
                .Be(variableProvider.UtcNow.AddTicks(tokenTuration.Ticks));
            bewit.Payload.Should().BeEquivalentTo(payload);
            bewit.Hash.Should()
                .Be("724e7acc-be57-49a1-8195-46a03c6271c6__2018-06-06T01:02:01.0010000Z__{\"Bar\":1}");
        }

        [Fact]
        public async Task GenerateBewitAsync_WithDifferentPayload_ShouldGenerateDifferentBewit()
        {
            //Arrange
            var variableProvider = new MockHelper.MockedVariablesProvider();
            var tokenTuration = TimeSpan.FromMinutes(1);
            IBewitTokenGenerator<Foo> provider =
                CreateGenerator<Foo>(
                    tokenTuration,
                    MockHelper.GetMockedCrpytoService<Foo>(),
                    variableProvider);
            var payload = new Foo
            {
                Bar = 5
            };

            //Act
            BewitToken<Foo> token =
                await provider.GenerateBewitTokenAsync(payload,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            Bewit<Foo> bewit = GetBewitFromToken(token);
            bewit.Token.Nonce.Should().Be(variableProvider.NextToken.ToString());
            bewit.Token.ExpirationDate.Should()
                .Be(variableProvider.UtcNow.AddTicks(tokenTuration.Ticks));
            bewit.Payload.Should().BeEquivalentTo(payload);
            bewit.Hash.Should()
                .Be("724e7acc-be57-49a1-8195-46a03c6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":5}");
        }

        [Fact]
        public void GenerateBewitAsync_WithNullPayload_ShouldThrow()
        {
            //Arrange
            var variableProvider = new MockHelper.MockedVariablesProvider();
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var tokenTuration = TimeSpan.FromMinutes(1);
            IBewitTokenGenerator<Foo> provider = CreateGenerator<Foo>(
                tokenTuration,
                cryptoService,
                variableProvider);

            //Act
            Func<Task> generateBewit = async () =>
                await provider.GenerateBewitTokenAsync(null,
                    new Dictionary<string, object>(),
                    CancellationToken.None);

            //Assert
            generateBewit.Should().Throw<ArgumentNullException>();
        }

        private IBewitTokenGenerator<T> CreateGenerator<T>(
            TimeSpan tokenDuration,
            ICryptographyService cryptographyService,
            IVariablesProvider variablesProvider)
        {
            var context = new BewitPayloadContext(typeof(T));
            context.SetCryptographyService(() => cryptographyService);
            context.SetVariablesProvider(() => variablesProvider);
            context.SetRepository(() => new DefaultNonceRepository());

            var options = new BewitOptions { TokenDuration = tokenDuration };
            return new BewitTokenGenerator<T>(options, context);
        }

        private Bewit<T> GetBewitFromToken<T>(BewitToken<T> bewit)
        {
            var base64Bewit = bewit.ToString();
            var serializedBewit = Encoding.UTF8.GetString(Convert.FromBase64String(base64Bewit));

            return JsonConvert.DeserializeObject<Bewit<T>>(serializedBewit);
        }

        #endregion
    }
}
