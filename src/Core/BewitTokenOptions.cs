namespace Bewit;

/// <summary>
/// Per-call options for token generation, allowing overrides of global <see cref="BewitOptions"/>.
/// </summary>
/// <example>
/// <code>
/// var token = await generator.GenerateBewitTokenAsync(
///     payload,
///     new BewitTokenOptions
///     {
///         Duration = TimeSpan.FromHours(24),
///         ExtraProperties = new() { ["tenantId"] = "abc" },
///         Identifier = "user-123"
///     },
///     cancellationToken);
/// </code>
/// </example>
public sealed class BewitTokenOptions
{
    /// <summary>
    /// Override the default token duration for this specific token.
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Additional metadata to store with the token's nonce record.
    /// </summary>
    public Dictionary<string, object>? ExtraProperties { get; set; }

    /// <summary>
    /// An identifier for bulk invalidation (e.g., user ID, share link ID).
    /// When set, all tokens with the same identifier can be revoked at once.
    /// </summary>
    public string? Identifier { get; set; }
}
