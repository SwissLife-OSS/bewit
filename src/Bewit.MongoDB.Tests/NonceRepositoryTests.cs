using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using FluentAssertions;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.MongoDB.Tests
{
    public class NonceRepositoryTests : IClassFixture<MongoResource>
    {
        private readonly MongoResource _mongoResource;

        public class Bar
        {
            public int Baz { get; set; }
        }

        static NonceRepositoryTests()
        {
            NonceRepository.Initialize();
        }

        public NonceRepositoryTests(MongoResource mongoResource)
        {
            _mongoResource = mongoResource;
        }

        [Fact]
        public async Task InsertOneAsync_WithNonceDerivate_ShouldStoreNonce()
        {
            //Arrange
            IMongoDatabase database = _mongoResource.CreateDatabase();
            var repository = new NonceRepository(database, nameof(Token));
            var token = "myToken";
            var expirationDate = DateTime.UtcNow;
            var nonce = new Bewit<Bar>(token, expirationDate, new Bar(), "hash");

            //Act
            await repository.InsertOneAsync(nonce, CancellationToken.None);

            //Assert
            var collection = database.GetCollection<Token>(nameof(Token));
            List<Token> items = (
                await collection.FindAsync(
                    Builders<Token>.Filter.Empty,
                    cancellationToken: CancellationToken.None)
            ).ToList();
            items.Should().ContainSingle();
            items.First().Nonce.Should().Be(token);
            items.First().ExpirationDate.Date.Should().Be(expirationDate.Date);
        }

        [Fact]
        public async Task FindOneAndDeleteAsync_WithExistingNonceDerivate_ShouldRetrieveAndDeleteNonce()
        {
            //Arrange
            IMongoDatabase database = _mongoResource.CreateDatabase();
            var repository = new NonceRepository(database, nameof(Token));
            var token = "myToken";
            var expirationDate = DateTime.UtcNow;
            var nonce = new Bewit<Bar>(token, expirationDate, new Bar(), "hash");
            var collection = database.GetCollection<Token>(nameof(Token));
            await collection.InsertOneAsync(
                nonce, new InsertOneOptions(), CancellationToken.None);

            //Act
            var returnedNonce =
                await repository.FindOneAndDeleteAsync(token,
                    CancellationToken.None);

            //Assert
            List<Token> items = (
                await collection.FindAsync(
                    Builders<Token>.Filter.Empty,
                    cancellationToken: CancellationToken.None)
            ).ToList();
            items.Should().BeEmpty();
            returnedNonce.Nonce.Should().Be(token);
            returnedNonce.ExpirationDate.Date.Should().Be(expirationDate.Date);
        }

        [Fact]
        public async Task FindOneAndDeleteAsync_WithNonExistingNonceDerivate_ShouldRetrieveAndDeleteNonce()
        {
            //Arrange
            IMongoDatabase database = _mongoResource.CreateDatabase();
            var repository = new NonceRepository(database, nameof(Token));
            var token = "myToken";

            //Act
            var returnedNonce =
                await repository.FindOneAndDeleteAsync(token,
                    CancellationToken.None);

            //Assert
            var collection = database.GetCollection<Token>(nameof(Token));
            List<Token> items = (
                await collection.FindAsync(
                    Builders<Token>.Filter.Empty,
                    cancellationToken: CancellationToken.None)
            ).ToList();
            items.Should().BeEmpty();
            returnedNonce.Should().BeNull();
        }
    }
}
