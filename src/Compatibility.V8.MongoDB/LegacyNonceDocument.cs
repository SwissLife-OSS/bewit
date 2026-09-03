using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bewit.Compatibility.V8.MongoDB;

internal sealed class LegacyNonceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Nonce { get; set; }

    [BsonElement("exp")]
    public DateTime? ExpirationDate { get; set; }

    [BsonElement("ident")]
    public string? Identifier { get; set; }

    [BsonElement("del")]
    public bool IsDeleted { get; set; }

    [BsonElement("extra")]
    public Dictionary<string, object>? ExtraProperties { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
