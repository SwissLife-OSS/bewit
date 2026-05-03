using Microsoft.AspNetCore.Http;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitTokenHeaderRequestMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(
                BewitTokenConstants.HeaderName, out var headerValues)
            && headerValues.Count > 0)
        {
            string? token = headerValues[0];

            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Items[BewitTokenConstants.ContextKey] = token;
            }
        }

        await next(context);
    }
}
