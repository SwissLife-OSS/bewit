using System.ComponentModel;

namespace Bewit.Extensions.HotChocolate;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use BewitTokenExtractionOptions instead.")]
internal static class BewitTokenConstants
{
    public const string HeaderName = "bewitToken";
    public const string ContextKey = "bewitToken";
}
