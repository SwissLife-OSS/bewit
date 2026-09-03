using Bewit.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.AspNetCore;

public sealed class BewitEndpointFilter<T> : IEndpointFilter
    where T : notnull
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        HttpContext httpContext = context.HttpContext;
        IOptions<BewitAspNetCoreOptions> options = httpContext.RequestServices
            .GetRequiredService<IOptions<BewitAspNetCoreOptions>>();

        string? tokenString = httpContext.GetBewitToken(options.Value);

        if (string.IsNullOrWhiteSpace(tokenString))
        {
            return Results.Unauthorized();
        }

        try
        {
            IBewitTokenValidator<T> validator = httpContext.RequestServices
                .GetRequiredService<IBewitTokenValidator<T>>();

            T payload = await validator.ValidateAsync(
                new BewitToken<T>(tokenString),
                httpContext.RequestAborted);

            httpContext.SetBewitPayload(payload);
        }
        catch (BewitException)
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
