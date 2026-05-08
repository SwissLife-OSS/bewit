namespace Bewit.Exceptions;

/// <summary>
/// Thrown when a bewit token's HMAC hash does not match the expected value,
/// indicating the token has been tampered with or the secret is incorrect.
/// </summary>
public sealed class BewitInvalidException()
    : BewitException("The bewit token is invalid (hash mismatch).");
