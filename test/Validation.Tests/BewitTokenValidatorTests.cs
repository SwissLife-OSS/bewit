using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Validation.Exceptions;
using FluentAssertions;
using Xunit;

namespace Bewit.Validation.Tests
{
    public class BewitTokenValidatorTests
    {
        public class Foo
        {
            public int Bar { get; set; }
        }

        [Fact]
        public async Task ValidateBewit_WithPayload_ShouldGenerateBewit()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            var provider =
                new BewitTokenValidatorAccessor<Foo>(
                    cryptoService, new MockHelper.MockedVariablesProvider(), nonceRepository);
            var payload = new Foo
            {
                Bar = 1
            };

            var token = Token.Create(
                "724e7acc-be57-49a1-8195-46a03c6271c6",
                new DateTime(2017, 1, 1, 1, 2, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                payload,
                "724e7acc-be57-49a1-8195-46a03c6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            //Act
            Bewit<Foo> bewit2 = await provider.InvokeValidateBewitAsync(bewit, CancellationToken.None);

            //Assert
            bewit2.Should().Be(bewit);
        }

        [Fact]
        public async Task ValidateBewit_WithAlteredHash_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            var provider =
                new BewitTokenValidatorAccessor<Foo>(
                    cryptoService, new MockHelper.MockedVariablesProvider(), nonceRepository);
            var payload = new Foo
            {
                Bar = 1
            };

            var token = Token.Create(
                "724e7acc-be57-49a1-8195-46a03c6271c6",
                new DateTime(2017, 1, 1, 1, 2, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                payload,
                "724e7acc-be57-zjzgjc6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            //Act
            Func<Task> validateBewit = async () =>
                await provider.InvokeValidateBewitAsync(bewit,
                    CancellationToken.None);

            //Assert
            await validateBewit.Should().ThrowAsync<BewitInvalidException>();
        }

        [Fact]
        public async Task ValidateBewit_WithAlteredPayload_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            var provider =
                new BewitTokenValidatorAccessor<Foo>(
                    cryptoService, new MockHelper.MockedVariablesProvider(), nonceRepository);

            var token = Token.Create(
                "724e7acc-be57-49a1-8195-46a03c6271c6",
                new DateTime(2017, 1, 1, 1, 2, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                new Foo
                {
                    Bar = 2
                },
                "724e7acc-be57-49a1-8195-46a03c6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            //Act
            Func<Task> validateBewit = async () =>
                await provider.InvokeValidateBewitAsync(bewit,
                    CancellationToken.None);

            //Assert
            await validateBewit.Should().ThrowAsync<BewitInvalidException>();
        }

        [Fact]
        public async Task ValidateBewit_WithAlteredExpirationDate_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            var provider =
                new BewitTokenValidatorAccessor<Foo>(
                    cryptoService, new MockHelper.MockedVariablesProvider(), nonceRepository);
            var payload = new Foo
            {
                Bar = 1
            };

            var token = Token.Create(
                "724e7acc-be57-49a1-8195-46a03c6271c6",
                new DateTime(2029, 1, 1, 1, 2, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                payload,
                "724e7acc-be57-49a1-8195-46a03c6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            //Act
            Func<Task> validateBewit = async () =>
                await provider.InvokeValidateBewitAsync(bewit,
                    CancellationToken.None);

            //Assert
            await validateBewit.Should().ThrowAsync<BewitInvalidException>();
        }

        [Fact]
        public async Task ValidateBewit_WithAlteredToken_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            var provider =
                new BewitTokenValidatorAccessor<Foo>(
                    cryptoService, new MockHelper.MockedVariablesProvider(), nonceRepository);
            var payload = new Foo
            {
                Bar = 1
            };

            var token = Token.Create(
                "esfesf",
                new DateTime(2017, 1, 1, 1, 2, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                payload,
                "724e7acc-be57-49a1-8195-46a03c6271c6__2017-01-01T01:02:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            //Act
            Func<Task> validateBewit = async () =>
                await provider.InvokeValidateBewitAsync(bewit,
                    CancellationToken.None);

            //Assert
            await validateBewit.Should().ThrowAsync<BewitInvalidException>();
        }

        [Fact]
        public async Task ValidateBewit_Expired_ShouldThrow()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            var provider =
                new BewitTokenValidatorAccessor<Foo>(
                    cryptoService, new MockHelper.MockedVariablesProvider(), nonceRepository);
            var payload = new Foo
            {
                Bar = 1
            };

            var token = Token.Create(
                "724e7acc-be57-49a1-8195-46a03c6271c6",
                new DateTime(2016, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                payload,
                "724e7acc-be57-49a1-8195-46a03c6271c6__2016-01-01T01:01:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            //Act
            Func<Task> validateBewit = async () => await
                provider.InvokeValidateBewitAsync(bewit,
                    CancellationToken.None);

            //Assert
            await validateBewit.Should().ThrowAsync<BewitExpiredException>();
        }

        [Fact]
        public async Task ValidateBewitToken_WithPayload_ShouldReturnPayload()
        {
            //Arrange
            ICryptographyService cryptoService =
                MockHelper.GetMockedCrpytoService<Foo>();
            var nonceRepository = new DefaultNonceRepository();
            BewitPayloadContext context = new BewitPayloadContext(typeof(Foo))
                .SetCryptographyService(() => cryptoService)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                .SetRepository(() => nonceRepository);
            var provider = new BewitTokenValidator<Foo>(context);
            var payload = new Foo
            {
                Bar = 1
            };
            var token = Token.Create(
                "724e7acc-be57-49a1-8195-46a03c6271c6",
                new DateTime(2016, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc));
            var bewit = new Bewit<Foo>(
                token,
                payload,
                "724e7acc-be57-49a1-8195-46a03c6271c6__2016-01-01T01:01:01.0010000Z__{\"Bar\":1}");
            await nonceRepository.InsertOneAsync(bewit.Token, default);

            var bewitToken = new BewitToken<Foo>(
                "eyJQYXlsb2FkIjp7IkJhciI6MX0sIkhhc2giOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzZfXzIwMTctMDEtMDFUMDE6MDI6MDEuMDAxMDAwMFpfX3tcIkJhclwiOjF9IiwiTm9uY2UiOiI3MjRlN2FjYy1iZTU3LTQ5YTEtODE5NS00NmEwM2M2MjcxYzYiLCJFeHBpcmF0aW9uRGF0ZSI6IjIwMTctMDEtMDFUMDE6MDI6MDEuMDAxWiJ9");

            //Act
            Foo payload2 =
                await provider.ValidateBewitTokenAsync(bewitToken, CancellationToken.None);

            //Assert
            payload2.Bar.Should().Be(payload.Bar);
        }
    }
}
