namespace Bewit.AspNetCore;

public sealed class BewitAspNetCoreOptions
{
    public string HeaderName { get; set; } = "bewitToken";
    public string QueryParameterName { get; set; } = "bewit";
    public BewitTokenSource Sources { get; set; } = BewitTokenSource.HeaderAndQueryString;
}
