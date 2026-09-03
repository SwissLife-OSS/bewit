using Bewit.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitEndpointExtensions
{
    public static IApplicationBuilder UseBewitEndpointAuthorization<T>(
        this IApplicationBuilder app)
        where T : notnull
    {
        return app.UseMiddleware<BewitEndpointMiddleware<T>>();
    }

    public static RouteHandlerBuilder AddBewitAuthorization<T>(this RouteHandlerBuilder builder)
        where T : notnull
    {
        return builder.AddEndpointFilter<BewitEndpointFilter<T>>();
    }

    public static RouteGroupBuilder AddBewitAuthorization<T>(this RouteGroupBuilder builder)
        where T : notnull
    {
        builder.AddEndpointFilter<BewitEndpointFilter<T>>();

        return builder;
    }
}
