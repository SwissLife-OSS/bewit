namespace Bewit;

public sealed record BewitTokenRejectedContext(
    string Purpose,
    BewitTokenFormat? Format,
    BewitTokenRejectionReason Reason,
    Exception? Exception = null);

public enum BewitTokenRejectionReason
{
    Invalid = 0,
    NotFound = 1,
    Revoked = 2,
    AlreadyConsumed = 3,
    PolicyFailed = 4
}
