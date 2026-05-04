using Bewit.Extensions.HotChocolate;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitHotChocolateServiceCollectionExtensions
{
    public static IApplicationBuilder UseBewitTokenExtraction(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<BewitTokenExtractionMiddleware>();
    }

    [Obsolete("Use UseBewitTokenExtraction instead.")]
    public static IApplicationBuilder UseBewitTokenHeaderExtraction(
        this IApplicationBuilder app)
    {
        return app.UseBewitTokenExtraction();
    }
}
