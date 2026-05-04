namespace Bewit;

public sealed class BewitTokenExtractionOptions
{
    public string HeaderName { get; set; } = "bewitToken";

    public string QueryParamName { get; set; } = "bewit";

    internal string ContextKey { get; set; } = "bewitToken";
}
