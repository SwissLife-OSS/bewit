using Squadron;
using Xunit;

namespace Bewit.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationFixture : ICollectionFixture<MongoReplicaSetResource>
{
    public const string Name = "MongoDB";
}
