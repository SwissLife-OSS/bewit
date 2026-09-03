namespace Bewit;

public sealed record BewitTokenRegistration<TPayload>(string Purpose)
    where TPayload : notnull;
