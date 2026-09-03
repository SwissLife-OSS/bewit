namespace Bewit.Exceptions;

/// <summary>
/// Thrown when a bewit token is malformed, unsupported, or fails signature verification.
/// </summary>
public sealed class BewitInvalidException()
    : BewitException("The bewit token is invalid.");
