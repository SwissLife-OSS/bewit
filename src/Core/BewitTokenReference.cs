namespace Bewit;

public readonly record struct BewitTokenReference(
    BewitTokenFormat Format,
    string Purpose,
    Guid TokenId);
