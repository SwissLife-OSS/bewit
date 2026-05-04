using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitTokenExtractionMiddleware(
    RequestDelegate next,
    IOptions<BewitTokenExtractionOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        BewitTokenExtractionOptions config = options.Value;

        string? token = null;

        if (context.Request.Headers.TryGetValue(
                config.HeaderName, out var headerValues)
            && headerValues.Count > 0)
        {
            token = headerValues[0];
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            token = context.Request.Query[config.QueryParamName];
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            context.Items[config.ContextKey] = token;
        }

        await next(context);
    }
}
