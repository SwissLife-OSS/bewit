using Bewit.Extensions.HotChocolate;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitHotChocolateServiceCollectionExtensions
{
    public static IApplicationBuilder UseBewitTokenHeaderExtraction(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<BewitTokenHeaderRequestMiddleware>();
    }
}
