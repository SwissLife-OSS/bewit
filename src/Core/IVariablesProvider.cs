namespace Bewit;

/// <summary>
/// Provides variable values for token generation, abstracted for testability.
/// </summary>
public interface IVariablesProvider
{
    /// <summary>
    /// Current UTC time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Generates a new unique nonce identifier.
    /// </summary>
    Guid NextToken { get; }
}
