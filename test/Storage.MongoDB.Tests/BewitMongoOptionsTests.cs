using FluentAssertions;
using Xunit;

namespace Bewit.Storage.MongoDB.Tests
{
    public class BewitMongoOptionsTests
    {
        [Fact]
        public void FieldInit_DummyValues_ShouldRestitureDummyValues()
        {
            //Arrange
            const string collectionName = "foo";
            const string connectionString = "bar";
            const string databaseName = "baz";

            //Act
            var options = new MongoNonceOptions
            {
                ConnectionString = connectionString,
                DatabaseName = databaseName,
                CollectionName = collectionName
            };

            //Assert
            options.CollectionName.Should().Be(collectionName);
            options.ConnectionString.Should().Be(connectionString);
            options.DatabaseName.Should().Be(databaseName);
        }
    }
}
