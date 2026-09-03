namespace Bewit.Exceptions;

public sealed class BewitPolicyException(Exception innerException)
    : BewitException("A bewit validation policy rejected the token.", innerException);
