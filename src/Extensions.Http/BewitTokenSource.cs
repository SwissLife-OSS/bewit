namespace Bewit.AspNetCore;

[Flags]
public enum BewitTokenSource
{
    Header = 1,
    QueryString = 2,
    HeaderAndQueryString = Header | QueryString
}
