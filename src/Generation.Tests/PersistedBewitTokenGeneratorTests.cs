using System;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bewit.Generation.Tests
{
    public class PersistedBewitTokenGeneratorTests
    {
        private class MockedVariablesProvider : IVariablesProvider
        {
            public DateTime UtcNow =>
                new DateTime(2017, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            public Guid NextToken =>
                new Guid("724e7acc-be57-49a1-8195-46a03c6271c6");
        }

        public class Bar
        {
            public string Baz { get; set; }
        }

        [Fact]
        public void Constructor_WithAllMandatoryOptions_ShouldReturnInstance()
        {
            //Arrange
            var repository = new Mock<INonceRepository>();
            var secret = "123";

            //Act
            var provider = new PersistedBewitTokenGenerator<Bar>(
                default, 
                new HmacSha256CryptographyService(secret),
                new MockedVariablesProvider(), 
                repository.Object);

            //Assert
            provider.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateBewitTokenAsync_WithLegitPayload_ShouldStoreNonceAndReturnBewitToken()
        {
            //Arrange
            Token insertedToken = null;
            var repository = new Mock<INonceRepository>();
            repository
                .Setup(r => r.InsertOneAsync(It.IsAny<Token>(),
                    It.IsAny<CancellationToken>()))
                .Callback((Token n, CancellationToken c) => insertedToken = n)
                .Returns(Task.CompletedTask);

            var secret = "123";
            var provider =
                new PersistedBewitTokenGenerator<Bar>(
                    default,
                    new HmacSha256CryptographyService(secret),
                    new MockedVariablesProvider(), 
                    repository.Object);
            var payload = new Bar
            {
                Baz = "foo"
            };

            //Act
            BewitToken<Bar> res =
                await provider.GenerateBewitTokenAsync(payload,
                    CancellationToken.None);

            //Assert
            res.ToString().Should().Be("eyJQYXlsb2FkIjp7IkJheiI6ImZvbyJ9LCJIYXNoIjoiSEdUaDAxdlFBMzVzNCtEMzZHT1NKNnhCM25IOVFNLytkVWZTNktHc3RmWT0iLCJOb25jZSI6IjcyNGU3YWNjLWJlNTctNDlhMS04MTk1LTQ2YTAzYzYyNzFjNiIsIkV4cGlyYXRpb25EYXRlIjoiMjAxNy0wMS0wMVQwMTowMjowMS4wMDFaIn0=");
            insertedToken.Nonce.Should().Be("724e7acc-be57-49a1-8195-46a03c6271c6");
            insertedToken.ExpirationDate.Should()
                .Be(new DateTime(2017, 1, 1, 1, 2, 1, 1, DateTimeKind.Utc));
        }
    }
}
