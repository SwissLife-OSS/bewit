namespace Bewit.Exceptions;

/// <summary>
/// Base exception for all bewit-related errors.
/// </summary>
public abstract class BewitException(string message)
    : Exception(message);
