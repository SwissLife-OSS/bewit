using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bewit.MongoDB;

internal sealed class BewitTokenDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid TokenId { get; set; }

    public required string Purpose { get; set; }
    public BewitTokenFormat Format { get; set; }
    public DateTime ExpiresAt { get; set; }
    public BewitTokenUsage Usage { get; set; }
    public BewitTokenStatus Status { get; set; }
    public string? Identifier { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? MetadataJson { get; set; }

    public static BewitTokenDocument FromRecord(BewitTokenRecord record) => new()
    {
        TokenId = record.Reference.TokenId,
        Purpose = record.Reference.Purpose,
        Format = record.Reference.Format,
        ExpiresAt = record.ExpiresAt.UtcDateTime,
        Usage = record.Usage,
        Status = record.Status,
        Identifier = record.Identifier,
        CreatedAt = record.CreatedAt.UtcDateTime,
        ConsumedAt = record.ConsumedAt?.UtcDateTime,
        RevokedAt = record.RevokedAt?.UtcDateTime,
        MetadataJson = record.Metadata is null ? null : JsonSerializer.Serialize(record.Metadata)
    };

    public BewitTokenRecord ToRecord() => new(
        new BewitTokenReference(Format, Purpose, TokenId),
        new DateTimeOffset(DateTime.SpecifyKind(ExpiresAt, DateTimeKind.Utc)),
        Usage,
        Status,
        Identifier,
        new DateTimeOffset(DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc)),
        ConsumedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(ConsumedAt.Value, DateTimeKind.Utc)),
        RevokedAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(RevokedAt.Value, DateTimeKind.Utc)),
        MetadataJson is null
            ? null
            : JsonSerializer.Deserialize<IReadOnlyDictionary<string, JsonElement>>(MetadataJson));
}
