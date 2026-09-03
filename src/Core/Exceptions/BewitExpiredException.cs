namespace Bewit.Exceptions;

/// <summary>
/// Thrown when a bewit token has expired.
/// The signed embedded expiration or server-controlled record has passed.
/// </summary>
public sealed class BewitExpiredException()
    : BewitException("The bewit token has expired.");
