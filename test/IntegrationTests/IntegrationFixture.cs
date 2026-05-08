using Squadron;
using Xunit;

namespace Bewit.IntegrationTests;

public static class TestCollectionNames
{
    public const string Integration = "Integration";
}

[CollectionDefinition(TestCollectionNames.Integration)]
public class IntegrationFixture :
    ICollectionFixture<MongoReplicaSetResource>;
