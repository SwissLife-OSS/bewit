using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bewit.Exceptions;

namespace Bewit;

internal sealed class V9BewitTokenCodec<TPayload>(IOptions<BewitOptions> options)
    : IBewitTokenCodec<TPayload>
    where TPayload : notnull
{
    internal const string Prefix = "bwt1.";
    private readonly BewitOptions _options = options.Value;

    public BewitTokenFormat Format => BewitTokenFormat.V9;
    public bool CanRead(string token) => token.StartsWith(Prefix, StringComparison.Ordinal);

    public string Encode(
        BewitTokenReference reference,
        TPayload payload,
        BewitExpirationMode expirationMode,
        DateTimeOffset? embeddedExpiration)
    {
        byte[] envelope = SerializeEnvelope(
            _options.CurrentKeyId, reference, payload, expirationMode, embeddedExpiration);
        byte[] signature = Sign(envelope, GetKey(_options.CurrentKeyId));
        return $"{Prefix}{Base64Url.Encode(envelope)}.{Base64Url.Encode(signature)}";
    }

    public BewitDecodedToken<TPayload> DecodeAndVerify(string token)
    {
        try
        {
            if (!CanRead(token)
                || Encoding.UTF8.GetByteCount(token) > _options.MaximumTokenSizeBytes)
            {
                throw new BewitInvalidException();
            }

            string[] parts = token.Split('.');
            if (parts.Length != 3 || parts[0] != "bwt1")
            {
                throw new BewitInvalidException();
            }

            byte[] envelopeBytes = Base64Url.Decode(parts[1]);
            byte[] actualSignature = Base64Url.Decode(parts[2]);
            using JsonDocument document = JsonDocument.Parse(envelopeBytes);
            JsonElement root = document.RootElement;
            string keyId = RequiredString(root, "kid");
            byte[] expectedSignature = Sign(envelopeBytes, GetKey(keyId));
            if (!CryptographicOperations.FixedTimeEquals(actualSignature, expectedSignature))
            {
                throw new BewitInvalidException();
            }

            if (root.GetProperty("v").GetInt32() != 1)
            {
                throw new BewitInvalidException();
            }

            string purpose = RequiredString(root, "purpose");
            Guid tokenId = root.GetProperty("tokenId").GetGuid();
            var mode = (BewitExpirationMode)root.GetProperty("mode").GetInt32();
            if (!Enum.IsDefined(mode))
            {
                throw new BewitInvalidException();
            }

            DateTimeOffset? expiration = root.TryGetProperty("expiresAt", out JsonElement expiry)
                && expiry.ValueKind != JsonValueKind.Null
                ? expiry.GetDateTimeOffset()
                : null;
            if ((mode == BewitExpirationMode.SelfContained) != expiration.HasValue)
            {
                throw new BewitInvalidException();
            }

            TPayload payload = root.GetProperty("payload").Deserialize<TPayload>()
                ?? throw new BewitInvalidException();
            return new BewitDecodedToken<TPayload>(
                new BewitTokenReference(Format, purpose, tokenId),
                payload,
                mode,
                expiration);
        }
        catch (BewitInvalidException)
        {
            throw;
        }
        catch (Exception exception) when (exception is FormatException
            or JsonException
            or KeyNotFoundException
            or InvalidOperationException)
        {
            throw new BewitInvalidException();
        }
    }

    private byte[] GetKey(string keyId)
    {
        if (!_options.SigningKeys.TryGetValue(keyId, out string? key))
        {
            throw new BewitInvalidException();
        }
        return Encoding.UTF8.GetBytes(key);
    }

    private static byte[] Sign(ReadOnlySpan<byte> value, byte[] key) =>
        HMACSHA256.HashData(key, value);

    private static string RequiredString(JsonElement root, string property)
    {
        string? value = root.GetProperty(property).GetString();
        return string.IsNullOrWhiteSpace(value) ? throw new BewitInvalidException() : value;
    }

    private static byte[] SerializeEnvelope(
        string keyId,
        BewitTokenReference reference,
        TPayload payload,
        BewitExpirationMode expirationMode,
        DateTimeOffset? embeddedExpiration)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber("v", 1);
            writer.WriteString("kid", keyId);
            writer.WriteString("purpose", reference.Purpose);
            writer.WriteNumber("mode", (int)expirationMode);
            writer.WriteString("tokenId", reference.TokenId);
            if (embeddedExpiration is { } expiresAt)
            {
                writer.WriteString("expiresAt", expiresAt);
            }
            else
            {
                writer.WriteNull("expiresAt");
            }
            writer.WritePropertyName("payload");
            JsonSerializer.Serialize(writer, payload);
            writer.WriteEndObject();
        }
        return stream.ToArray();
    }
}
