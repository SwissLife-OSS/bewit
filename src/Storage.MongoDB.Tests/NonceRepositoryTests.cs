using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Core;
using FluentAssertions;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.Storage.MongoDB.Tests
{
    public class NonceRepositoryTests : IClassFixture<MongoResource>
    {
        private readonly MongoResource _mongoResource;

        public class Bar
        {
            public int Baz { get; set; }
        }

        public class Bar2<T1, T2>
        {
            public T1 Baz { get; set; }
            public T2 Faz { get; set; }
        }

        static NonceRepositoryTests()
        {
            MongoNonceRepository.Initialize();
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
            var repository = new MongoNonceRepository(database, nameof(Token));
            var token = "myToken";
            DateTime expirationDate = DateTime.UtcNow;
            var nonce = new Bewit<Bar2<int, string>>(token, expirationDate, new Bar2<int, string>(), "hash");

            //Act
            await repository.InsertOneAsync(nonce, CancellationToken.None);

            //Assert
            IMongoCollection<Bewit<Bar2<int, string>>> collection = database.GetCollection<Bewit<Bar2<int, string>>>(nameof(Token));
            var items = (
                await collection.FindAsync(
                    Builders<Bewit<Bar2<int, string>>>.Filter.Empty,
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
            var repository = new MongoNonceRepository(database, nameof(Token));
            var token = "myToken";
            DateTime expirationDate = DateTime.UtcNow;
            var nonce = new Bewit<Bar>(token, expirationDate, new Bar(), "hash");
            IMongoCollection<Token> collection = database.GetCollection<Token>(nameof(Token));
            await collection.InsertOneAsync(
                nonce, new InsertOneOptions(), CancellationToken.None);

            //Act
            Token returnedNonce =
                await repository.TakeOneAsync(token,
                    CancellationToken.None);

            //Assert
            var items = (
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
            var repository = new MongoNonceRepository(database, nameof(Token));
            var token = "myToken";

            //Act
            Token returnedNonce =
                await repository.TakeOneAsync(token,
                    CancellationToken.None);

            //Assert
            IMongoCollection<Token> collection = database.GetCollection<Token>(nameof(Token));
            var items = (
                await collection.FindAsync(
                    Builders<Token>.Filter.Empty,
                    cancellationToken: CancellationToken.None)
            ).ToList();
            items.Should().BeEmpty();
            returnedNonce.Should().BeNull();
        }
    }
}
