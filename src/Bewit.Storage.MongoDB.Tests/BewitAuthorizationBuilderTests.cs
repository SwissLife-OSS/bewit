using System;
using System.Collections.Generic;
using Bewit.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Squadron;
using Xunit;

namespace Bewit.MongoDB.Tests
{
    public class BewitAuthorizationBuilderTests: IClassFixture<MongoResource>
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
            var builder = new BewitRegistrationBuilder();
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
            BewitRegistrationBuilder builder2 = builder.UseMongoPersistance(configuration);

            //Assert
            builder2.Should().Be(builder);
            builder2.GetRepository.Should().NotBeNull();
        }

        [Fact]
        public void UseMongoPersistance_WithMissingConnectionStringInConfiguration_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitRegistrationBuilder();
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
            Func<BewitRegistrationBuilder> builder2 = () => builder.UseMongoPersistance(configuration);

            //Assert
            builder2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UseMongoPersistance_WithMissingDatabaseNameInConfiguration_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitRegistrationBuilder();
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
            Func<BewitRegistrationBuilder> builder2 = () => builder.UseMongoPersistance(configuration);

            //Assert
            builder2.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UseMongoPersistance_WithOnlyMandatoryParameters_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitRegistrationBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();

            //Act
            BewitRegistrationBuilder builder2 = builder.UseMongoPersistance(
                new BewitMongoOptions
                {
                    ConnectionString = _mongoResource.ConnectionString,
                    DatabaseName = collection.Database.DatabaseNamespace.DatabaseName
                }
            );

            //Assert
            builder2.Should().Be(builder);
            builder2.GetRepository.Should().NotBeNull();
        }

        [Fact]
        public void UseMongoPersistance_WithOptionalCollectionNameParameter_ShouldInitAndReturnMongoNonceRepository()
        {
            //Arrange
            var builder = new BewitRegistrationBuilder();
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();

            //Act
            BewitRegistrationBuilder builder2 = builder.UseMongoPersistance(
                new BewitMongoOptions
                {
                    ConnectionString = _mongoResource.ConnectionString,
                    DatabaseName = collection.Database.DatabaseNamespace.DatabaseName,
                    CollectionName = "bar"
                }
            );

            //Assert
            builder2.Should().Be(builder);
            builder2.GetRepository.Should().NotBeNull();
        }

        [Fact]
        public void UseMongoPersistance_WithBuilderNull_ShouldThrowArgumentNullException()
        {
            //Arrange
            BewitRegistrationBuilder builder = null;
            IMongoCollection<Foo> collection = _mongoResource.CreateCollection<Foo>();

            //Act
            Func<BewitRegistrationBuilder> useRepository = ()
                => builder.UseMongoPersistance(
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
