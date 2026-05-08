using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bewit;

/// <summary>
/// HMAC-SHA256 implementation of <see cref="ICryptographyService"/>.
/// Uses System.Text.Json for payload serialization.
/// </summary>
internal sealed class HmacSha256CryptographyService : ICryptographyService
{
    private readonly byte[] _keyBytes;

    public HmacSha256CryptographyService(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException("Secret must not be empty.", nameof(secret));
        }

        _keyBytes = Encoding.UTF8.GetBytes(secret);
    }

    public string GetHash<T>(Guid nonce, DateTime? expirationDate, T payload) where T : notnull
    {
        var content = new
        {
            nonce,
            expirationDate,
            payload,
            type = typeof(T).FullName
        };

        var contentBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(content));

        var hashBytes = HMACSHA256.HashData(_keyBytes, contentBytes);

        return Convert.ToBase64String(hashBytes);
    }
}
