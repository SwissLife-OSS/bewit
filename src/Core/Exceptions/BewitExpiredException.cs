namespace Bewit.Exceptions;

/// <summary>
/// Thrown when a bewit token has expired.
/// For <see cref="ExpiryMode.SelfContained"/>, the embedded expiration date has passed.
/// For <see cref="ExpiryMode.ServerControlled"/>, the nonce record's expiry in the database has passed.
/// </summary>
public sealed class BewitExpiredException()
    : BewitException("The bewit token has expired.");
