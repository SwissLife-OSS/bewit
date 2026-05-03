using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Bewit;

/// <summary>
/// Configuration options for the Bewit token system.
/// Supports named options for multi-tenancy scenarios.
/// </summary>
/// <example>
/// <code>
/// services.AddBewit(bewit =>
/// {
///     bewit.ConfigureOptions(o =>
///     {
///         o.Secret = "my-secret-key";
///         o.TokenDuration = TimeSpan.FromMinutes(5);
///         o.ExpiryMode = ExpiryMode.ServerControlled;
///         o.SlidingWindow = TimeSpan.FromMinutes(30);
///     });
/// });
/// </code>
/// </example>
public sealed class BewitOptions
{
    /// <summary>
    /// Secret used for HMAC-SHA256 hash generation. Mandatory.
    /// </summary>
    [Required]
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Default duration of generated tokens.
    /// For <see cref="ExpiryMode.SelfContained"/>, this is the authoritative expiry baked into the token.
    /// For <see cref="ExpiryMode.ServerControlled"/>, this is a generous fallback safety net in the token;
    /// the nonce repository record holds the real expiry.
    /// Default is 60 seconds.
    /// </summary>
    public TimeSpan TokenDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Controls how token expiration is enforced.
    /// Default is <see cref="ExpiryMode.SelfContained"/>.
    /// </summary>
    public ExpiryMode ExpiryMode { get; set; } = ExpiryMode.SelfContained;

    /// <summary>
    /// When set, the nonce expiry is extended by this duration on each successful validation.
    /// Only applies when the nonce repository supports it (e.g., MongoDB storage).
    /// </summary>
    public TimeSpan? SlidingWindow { get; set; }
}

/// <summary>
/// Validates <see cref="BewitOptions"/> using data annotations and custom rules.
/// </summary>
[OptionsValidator]
public partial class BewitOptionsValidator : IValidateOptions<BewitOptions>
{
}
