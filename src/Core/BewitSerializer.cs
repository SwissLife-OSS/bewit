using System.Text;
using System.Text.Json;

namespace Bewit;

/// <summary>
/// Handles serialization of <see cref="Bewit{T}"/> to/from Base64-encoded JSON
/// for producing and consuming <see cref="BewitToken{T}"/> values.
/// </summary>
internal static class BewitSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a <see cref="Bewit{T}"/> to a Base64-encoded string.
    /// </summary>
    public static string Serialize<T>(Bewit<T> bewit) where T : notnull
    {
        var json = JsonSerializer.Serialize(bewit, JsonOptions);

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Deserializes a Base64-encoded string to a <see cref="Bewit{T}"/>.
    /// Returns <c>null</c> if deserialization fails.
    /// </summary>
    public static Bewit<T>? Deserialize<T>(string base64) where T : notnull
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            return JsonSerializer.Deserialize<Bewit<T>>(json, JsonOptions);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }
}
