using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using FluentAssertions;
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
            initProvider.Should().Throw<ArgumentException>();
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
            initProvider.Should().Throw<ArgumentException>();
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
            initProvider.Should().Throw<ArgumentException>();
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
                    CancellationToken.None);

            //Assert
            ((string)bewit).Should().Be("eyJQYXlsb2FkIjp7IkJhciI6MX0sIkhhc2giOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzZfXzIwMTctMDEtMDFUMDE6MDI6MDEuMDAxMDAwMFpfX3tcIkJhclwiOjF9IiwiTm9uY2UiOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzYiLCJFeHBpcmF0aW9uRGF0ZSI6IjIwMTctMDEtMDFUMDE6MDI6MDEuMDAxWiJ9");
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
                    CancellationToken.None);

            //Assert
            ((string)bewit).Should().Be("eyJQYXlsb2FkIjp7IkJhciI6MX0sIkhhc2giOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzZfXzIwMTgtMDYtMDZUMDE6MDI6MDEuMDAxMDAwMFpfX3tcIkJhclwiOjF9IiwiTm9uY2UiOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzYiLCJFeHBpcmF0aW9uRGF0ZSI6IjIwMTgtMDYtMDZUMDE6MDI6MDEuMDAxWiJ9");
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
                    CancellationToken.None);

            //Assert
            ((string)bewit).Should().Be("eyJQYXlsb2FkIjp7IkJhciI6NX0sIkhhc2giOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzZfXzIwMTctMDEtMDFUMDE6MDI6MDEuMDAxMDAwMFpfX3tcIkJhclwiOjV9IiwiTm9uY2UiOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzYiLCJFeHBpcmF0aW9uRGF0ZSI6IjIwMTctMDEtMDFUMDE6MDI6MDEuMDAxWiJ9");
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
            var provider =
                new BewitTokenGeneratorAccessor<Foo>(
                    tokenTuration, 
                    MockHelper.GetMockedCrpytoService<Foo>(),
                    variableProvider);
            var payload = new Foo
            {
                Bar = 1
            };

            //Act
            Bewit<Foo> bewit =
                await provider.InvokeGenerateBewitAsync(payload,
                    CancellationToken.None);

            //Assert
            bewit.Nonce.Should().Be(variableProvider.NextToken.ToString());
            bewit.ExpirationDate.Should()
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
            var provider =
                new BewitTokenGeneratorAccessor<Foo>(
                    tokenTuration, 
                    MockHelper.GetMockedCrpytoService<Foo>(),
                    variableProvider);
            var payload = new Foo
            {
                Bar = 1
            };

            //Act
            Bewit<Foo> bewit =
                await provider.InvokeGenerateBewitAsync(payload,
                    CancellationToken.None);

            //Assert
            bewit.Nonce.Should().Be(variableProvider.NextToken.ToString());
            bewit.ExpirationDate.Should()
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
            var provider =
                new BewitTokenGeneratorAccessor<Foo>(
                    tokenTuration, 
                    MockHelper.GetMockedCrpytoService<Foo>(),
                    variableProvider);
            var payload = new Foo
            {
                Bar = 5
            };

            //Act
            Bewit<Foo> bewit =
                await provider.InvokeGenerateBewitAsync(payload,
                    CancellationToken.None);

            //Assert
            bewit.Nonce.Should().Be(variableProvider.NextToken.ToString());
            bewit.ExpirationDate.Should()
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
            var provider = new BewitTokenGeneratorAccessor<Foo>(
                tokenTuration,
                cryptoService,
                variableProvider);

            //Act
            Func<Task> generateBewit = async () =>
                await provider.InvokeGenerateBewitAsync(null,
                    CancellationToken.None);

            //Assert
            generateBewit.Should().Throw<ArgumentNullException>();
        }

        #endregion
    }
}
