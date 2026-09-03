namespace Bewit.Compatibility.V8.MongoDB;

public sealed class BewitV8MongoOptions
{
    public string CollectionName { get; set; } = "bewit_nonces";
    public BewitTokenUsage Usage { get; set; } = BewitTokenUsage.SingleUse;
}
