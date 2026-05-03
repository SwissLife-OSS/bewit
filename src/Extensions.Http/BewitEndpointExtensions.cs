using Bewit.Http;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitEndpointExtensions
{
    public static IApplicationBuilder UseBewitEndpointAuthorization<T>(
        this IApplicationBuilder app)
        where T : notnull
    {
        return app.UseMiddleware<BewitEndpointMiddleware<T>>();
    }
}
