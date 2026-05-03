using System.Net;
using Bewit.Exceptions;
using Bewit.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Http;

internal sealed class BewitEndpointMiddleware<T>(RequestDelegate next) where T : notnull
{
    public async Task InvokeAsync(HttpContext context)
    {
        string? bewitToken = context.Request.Query["bewit"];

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return;
        }

        bewitToken = WebUtility.UrlDecode(bewitToken);

        var validator = context.RequestServices
            .GetRequiredService<IBewitTokenValidator<T>>();

        try
        {
            T payload = await validator.ValidateBewitTokenAsync(
                new BewitToken<T>(bewitToken),
                context.RequestAborted);

            var httpContextAccessor = context.RequestServices
                .GetRequiredService<IHttpContextAccessor>();

            httpContextAccessor.SetBewitPayload(payload);
        }
        catch (BewitException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            return;
        }

        await next(context);
    }
}
