namespace Bewit;

public enum BewitTokenConsumeStatus
{
    Consumed = 0,
    NotFound = 1,
    AlreadyConsumed = 2,
    Revoked = 3,
    Expired = 4
}

public sealed record BewitTokenConsumeResult(
    BewitTokenConsumeStatus Status,
    BewitTokenRecord? Record = null);
