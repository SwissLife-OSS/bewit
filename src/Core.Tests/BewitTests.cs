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
            string token = "bar";
            DateTime expirationDate = DateTime.UtcNow; 
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            var bewit = new Bewit<Foo>(token, expirationDate, payload, hash);
            
            //Assert
            bewit.Should().NotBeNull();
            bewit.Nonce.Should().Be(token);
            bewit.ExpirationDate.Should().Be(expirationDate);
            bewit.Payload.Should().BeEquivalentTo(payload);
            bewit.Hash.Should().Be(hash);
        }

        [Fact]
        public void Constructor_TokenNull_ShouldThrow()
        {
            //Arrange
            string token = null;
            DateTime expirationDate = DateTime.UtcNow; 
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, expirationDate, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_TokenWhitespace_ShouldThrow()
        {
            //Arrange
            string token = " ";
            DateTime expirationDate = DateTime.UtcNow; 
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, expirationDate, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_DateTimeDefault_ShouldThrow()
        {
            //Arrange
            string token = "bar";
            DateTime expirationDate = default; 
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, expirationDate, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_payloadNull_ShouldThrow()
        {
            //Arrange
            string token = "bar";
            DateTime expirationDate = DateTime.UtcNow;
            Foo payload = null;
            string hash = "123";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, expirationDate, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_HashNull_ShouldThrow()
        {
            //Arrange
            string token = "bar";
            DateTime expirationDate = DateTime.UtcNow; 
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = null;

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, expirationDate, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_HashWhitespace_ShouldThrow()
        {
            //Arrange
            string token = "bar";
            DateTime expirationDate = DateTime.UtcNow; 
            Foo payload = new Foo
            {
                Bar = 1
            };
            string hash = " ";

            //Act
            Action createBewit = () =>
                new Bewit<Foo>(token, expirationDate, payload, hash);

            //Assert
            createBewit.Should().Throw<ArgumentException>();
        }
    }
}
