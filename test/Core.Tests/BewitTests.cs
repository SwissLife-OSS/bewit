using System;
using FluentAssertions;
using Xunit;

namespace Bewit.Tests.Core
{
    public class BewitTests
    {
        internal class Foo
        {
            internal int Bar { get; set; }
        }

        [Fact]
        public void Constructor_AllParamsSet_ShouldInitializeProperly()
        {
            //Arrange
            string nextToken = "bar";
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";
            var token = Token.Create(nextToken, expirationDate);

            //Act
            var bewit = new Bewit<Foo>(token, payload, hash);

            //Assert
            bewit.Should().NotBeNull();
            bewit.Token.Nonce.Should().Be(nextToken);
            bewit.Token.ExpirationDate.Should().Be(expirationDate);
            bewit.Payload.Should().BeEquivalentTo(payload);
            bewit.Hash.Should().Be(hash);
        }

        [Fact]
        public void Constructor_TokenNull_ShouldThrow()
        {
            //Arrange
            string nextToken = null;
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(Token.Create(nextToken, expirationDate), payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_TokenWhitespace_ShouldThrow()
        {
            //Arrange
            string nextToken = " ";
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(Token.Create(nextToken, expirationDate), payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_DateTimeDefault_ShouldThrow()
        {
            //Arrange
            string nextToken = "bar";
            DateTime expirationDate = default;
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(Token.Create(nextToken, expirationDate), payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_PayloadNull_ShouldThrow()
        {
            //Arrange
            string nextToken = "bar";
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = null;
            string hash = "123";
            var token = Token.Create(nextToken, expirationDate);

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, payload!, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_HashNull_ShouldThrow()
        {
            //Arrange
            string nextToken = "bar";
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = null;
            var token = Token.Create(nextToken, expirationDate);

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, payload, hash!);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_HashWhitespace_ShouldThrow()
        {
            //Arrange
            string nextToken = "bar";
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = " ";
            var token = Token.Create(nextToken, expirationDate);

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }
    }
}
