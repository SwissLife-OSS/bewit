namespace Bewit.Exceptions;

/// <summary>
/// Thrown when a server-controlled token record is not found.
/// </summary>
public sealed class BewitNotFoundException()
    : BewitException("The bewit token record was not found.");
