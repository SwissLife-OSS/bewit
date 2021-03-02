using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bewit.Validation.Tests
{
    public class PersistedBewitTokenValidatorTests
    {
        public class Bar
        {
            public string Baz { get; set; }
        }

        [Fact]
        public async Task ValidateBewitTokenAsync_WithLegitPayload_ShouldStoreNonceAndReturnBewitToken()
        {
            //Arrange
            Token insertedToken = Token.Create("724e7acc-be57-49a1-8195-46a03c6271c6", DateTime.MaxValue);
            var repository = new Mock<INonceRepository>();
            repository
                .Setup(r => r.TakeOneAsync(It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns((string tok, CancellationToken c) =>
                {
                    if (tok == insertedToken.Nonce)
                    {
                        Token tmpToken = insertedToken;
                        insertedToken = null;
                        return new ValueTask<Token>(tmpToken);
                    }

                    return new ValueTask<Token>((Token)null);
                });
            BewitPayloadContext context = new BewitPayloadContext(typeof(Bar))
                .SetCryptographyService(MockHelper.GetMockedCrpytoService<Bar>)
                .SetVariablesProvider(() => new MockHelper.MockedVariablesProvider())
                .SetRepository(() => repository.Object);
            var provider =
                new BewitTokenValidator<Bar>(context);
            var payload = new Bar
            {
                Baz = "foo"
            };

            BewitToken<Bar> token = new BewitToken<Bar>("eyJQYXlsb2FkIjp7IkJheiI6ImZvbyJ9LCJIYXNoIjoiNzI0ZTdhY2MtYmU1Ny00OWExLTgxOTUtNDZhMDNjNjI3MWM2X18yMDE3LTAxLTAxVDAxOjAyOjAxLjAwMTAwMDBaX197XCJCYXpcIjpcImZvb1wifSIsIk5vbmNlIjoiNzI0ZTdhY2MtYmU1Ny00OWExLTgxOTUtNDZhMDNjNjI3MWM2IiwiRXhwaXJhdGlvbkRhdGUiOiIyMDE3LTAxLTAxVDAxOjAyOjAxLjAwMVoifQ==");

            //Act
            Bar payload2 =
                await provider.ValidateBewitTokenAsync(
                    token,
                    CancellationToken.None);

            //Assert
            payload2.Should().NotBeNull();
            payload2.Should().NotBe(payload);
            payload2.Should().BeEquivalentTo(payload);
        }
    }
}
