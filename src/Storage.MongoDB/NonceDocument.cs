using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bewit.Storage.MongoDB;

internal sealed class NonceDocument
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

    public Token ToToken() =>
        new(Nonce, ExpirationDate, Identifier, IsDeleted, ExtraProperties);

    public static NonceDocument FromToken(Token token) =>
        new()
        {
            Nonce = token.Nonce,
            ExpirationDate = token.ExpirationDate,
            Identifier = token.Identifier,
            IsDeleted = token.IsDeleted,
            ExtraProperties = token.ExtraProperties.Count > 0
                ? new Dictionary<string, object>(token.ExtraProperties)
                : null,
            CreatedAt = DateTime.UtcNow
        };
}
