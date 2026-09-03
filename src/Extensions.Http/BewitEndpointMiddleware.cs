using Bewit.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.AspNetCore;

internal sealed class BewitEndpointMiddleware<T>(
    RequestDelegate next,
    IOptions<BewitAspNetCoreOptions> options) where T : notnull
{
    public async Task InvokeAsync(HttpContext context)
    {
        string? bewitToken = context.GetBewitToken(options.Value);

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            context.Response.StatusCode = 401;

            return;
        }

        IBewitTokenValidator<T> validator = context.RequestServices
            .GetRequiredService<IBewitTokenValidator<T>>();

        try
        {
            T payload = await validator.ValidateAsync(
                new BewitToken<T>(bewitToken),
                context.RequestAborted);

            context.SetBewitPayload(payload);
        }
        catch (BewitException)
        {
            context.Response.StatusCode = 403;

            return;
        }

        await next(context);
    }
}
