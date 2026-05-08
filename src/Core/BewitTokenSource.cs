namespace Bewit;

[Flags]
public enum BewitTokenSource
{
    Header = 1,
    QueryString = 2,
    HeaderAndQueryString = Header | QueryString
}
