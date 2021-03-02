using System;
using System.Collections.Generic;
using Bewit.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.Storage.MongoDB.Tests
{
    public class BewitAuthorizationBuilderTests : IClassFixture<MongoResource>
    {
        public class Foo { }

        private readonly MongoResource _mongoResource;

        public BewitAuthorizationBuilderTests(MongoResource mongoResource)
        {
            _mongoResource = mongoResource;
        }

        [Fact]
        public void UseMongoPersistance_WithValidConfiguration_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitPayloadBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Mongo:ConnectionString",
                        _mongoResource.ConnectionString),
                    new KeyValuePair<string, string>("Bewit:Mongo:DatabaseName",
                        collection.Database.DatabaseNamespace.DatabaseName),
                })
                .Build();

            //Act
            builder.UseMongoPersistence(configuration);

            //Assert
            builder.Should().Be(builder);
            builder.CreateRepository.Should().NotBeNull();
        }

        [Fact]
        public void UseMongoPersistance_WithMissingConnectionStringInConfiguration_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitPayloadBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Mongo:fgfdg",
                        _mongoResource.ConnectionString),
                    new KeyValuePair<string, string>("Bewit:Mongo:DatabaseName",
                        collection.Database.DatabaseNamespace.DatabaseName),
                })
                .Build();

            //Act
            Action builder2 = () => builder.UseMongoPersistence(configuration);

            //Assert
            builder2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UseMongoPersistance_WithMissingDatabaseNameInConfiguration_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitPayloadBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("Bewit:Mongo:ConnectionString",
                        _mongoResource.ConnectionString),
                    new KeyValuePair<string, string>("Bewit:Mongo:sefef",
                        collection.Database.DatabaseNamespace.DatabaseName),
                })
                .Build();

            //Act
            Action builder2 = () => builder.UseMongoPersistence(configuration);

            //Assert
            builder2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UseMongoPersistance_WithOnlyMandatoryParameters_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitPayloadBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();

            //Act
            builder.UseMongoPersistence(
                new BewitMongoOptions
                {
                    ConnectionString = _mongoResource.ConnectionString,
                    DatabaseName = collection.Database.DatabaseNamespace.DatabaseName
                }
            );

            //Assert
            builder.Should().Be(builder);
            builder.CreateRepository.Should().NotBeNull();
        }

        [Fact]
        public void UseMongoPersistance_WithOptionalCollectionNameParameter_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitPayloadBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();

            //Act
            builder.UseMongoPersistence(
                new BewitMongoOptions
                {
                    ConnectionString = _mongoResource.ConnectionString,
                    DatabaseName = collection.Database.DatabaseNamespace.DatabaseName,
                    CollectionName = "bar"
                }
            );

            //Assert
            builder.Should().Be(builder);
            builder.CreateRepository.Should().NotBeNull();
        }

        [Fact]
        public void UseMongoPersistance_WithBuilderNull_ShouldThrowArgumentNullException()
        {
            //Arrange
            BewitPayloadBuilder builder = null;
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();

            //Act
            Action useRepository = ()
                => builder.UseMongoPersistence(
                    new BewitMongoOptions
                    {
                        ConnectionString = _mongoResource.ConnectionString,
                        DatabaseName = collection.Database.DatabaseNamespace.DatabaseName
                    }
                );

            //Assert
            useRepository.Should().Throw<ArgumentNullException>();
        }
    }
}
