using System.ComponentModel.DataAnnotations;

namespace Bewit;

public sealed class BewitTokenExtractionOptions
{
    [Required]
    public string HeaderName { get; set; } = "bewitToken";

    [Required]
    public string QueryParamName { get; set; } = "bewit";

    public BewitTokenSource Sources { get; set; } = BewitTokenSource.HeaderAndQueryString;

    internal string ContextKey => HeaderName;
}
