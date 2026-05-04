using Bewit.Exceptions;
using Bewit.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.Http;

internal sealed class BewitEndpointMiddleware<T>(
    RequestDelegate next,
    IOptions<BewitTokenExtractionOptions> options) where T : notnull
{
    public async Task InvokeAsync(HttpContext context)
    {
        BewitTokenExtractionOptions config = options.Value;

        string? bewitToken = null;

        if (context.Request.Headers.TryGetValue(
                config.HeaderName, out var headerValues)
            && headerValues.Count > 0)
        {
            bewitToken = headerValues[0];
        }

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            bewitToken = context.Request.Query[config.QueryParamName];
        }

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            context.Response.StatusCode = 401;

            return;
        }

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
            context.Response.StatusCode = 403;

            return;
        }

        await next(context);
    }
}
