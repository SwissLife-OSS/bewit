using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bewit.Exceptions;

namespace Bewit.Compatibility.V8;

internal sealed class V8BewitTokenCodec<TPayload>(
    BewitV8CompatibilityOptions<TPayload> configuredOptions,
    BewitTokenRegistration<TPayload> registration,
    TimeProvider timeProvider)
    : IBewitTokenCodec<TPayload>
    where TPayload : notnull
{
    private readonly BewitV8CompatibilityOptions options = configuredOptions.Value;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public BewitTokenFormat Format => BewitTokenFormat.V8;
    public bool CanRead(string token) => !token.StartsWith("bwt1.", StringComparison.Ordinal);

    public BewitDecodedToken<TPayload> DecodeAndVerify(string token)
    {
        try
        {
            if (options.AcceptUntil is { } deadline && timeProvider.GetUtcNow() > deadline)
            {
                throw new BewitInvalidException();
            }
            if (Encoding.UTF8.GetByteCount(token) > options.MaximumTokenSizeBytes)
            {
                throw new BewitInvalidException();
            }

            byte[] bytes = Convert.FromBase64String(token);
            LegacyBewit<TPayload> bewit = JsonSerializer.Deserialize<LegacyBewit<TPayload>>(bytes, JsonOptions)
                ?? throw new BewitInvalidException();
            DateTime? signedExpiration = options.ExpirationMode == BewitExpirationMode.SelfContained
                ? bewit.Token.ExpirationDate
                : null;
            byte[] expected = ComputeHash(bewit.Token.Nonce, signedExpiration, bewit.Payload);
            byte[] actual = Convert.FromBase64String(bewit.Hash);
            if (!CryptographicOperations.FixedTimeEquals(actual, expected))
            {
                throw new BewitInvalidException();
            }

            DateTimeOffset? expiration = signedExpiration is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(signedExpiration.Value, DateTimeKind.Utc));
            return new BewitDecodedToken<TPayload>(
                new BewitTokenReference(Format, registration.Purpose, bewit.Token.Nonce),
                bewit.Payload,
                options.ExpirationMode,
                expiration);
        }
        catch (BewitInvalidException)
        {
            throw;
        }
        catch (Exception exception) when (exception is FormatException or JsonException or InvalidOperationException)
        {
            throw new BewitInvalidException();
        }
    }

    private byte[] ComputeHash(Guid nonce, DateTime? expirationDate, TPayload payload)
    {
        var content = new { nonce, expirationDate, payload, type = options.PayloadTypeName };
        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content));
        return HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.Secret), bytes);
    }

    private sealed record LegacyBewit<T>(LegacyToken Token, T Payload, string Hash);
    private sealed record LegacyToken(Guid Nonce, DateTime? ExpirationDate);
}
