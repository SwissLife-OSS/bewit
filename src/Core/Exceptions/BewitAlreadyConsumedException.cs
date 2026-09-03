namespace Bewit.Exceptions;

public sealed class BewitAlreadyConsumedException()
    : BewitException("The single-use bewit token has already been consumed.");
