namespace Bewit.Exceptions;

/// <summary>
/// Thrown when a bewit token's nonce record is not found in the repository.
/// This can happen when a one-time token has already been consumed,
/// or the nonce has been explicitly deleted (revoked).
/// </summary>
public sealed class BewitNotFoundException()
    : BewitException("The bewit token was not found in the nonce repository.");
