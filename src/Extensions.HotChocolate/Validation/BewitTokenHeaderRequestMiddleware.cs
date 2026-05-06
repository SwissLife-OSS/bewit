using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitTokenExtractionMiddleware(
    RequestDelegate next,
    IOptions<BewitTokenExtractionOptions> options,
    ILogger<BewitTokenExtractionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        BewitTokenExtractionOptions config = options.Value;

        logger.LogDebug(
            "BewitTokenExtraction: Sources={Sources}, HeaderName={HeaderName}, QueryParam={QueryParam}, ContextKey={ContextKey}",
            config.Sources, config.HeaderName, config.QueryParamName, config.ContextKey);

        string? token = null;

        if (config.Sources.HasFlag(BewitTokenSource.Header))
        {
            bool hasHeader = context.Request.Headers.TryGetValue(
                config.HeaderName, out var headerValues);

            logger.LogDebug(
                "BewitTokenExtraction: Header '{HeaderName}' present={HasHeader}, count={Count}",
                config.HeaderName, hasHeader, hasHeader ? headerValues.Count : 0);

            if (hasHeader && headerValues.Count > 0)
            {
                token = headerValues[0];
            }
        }

        if (string.IsNullOrWhiteSpace(token)
            && config.Sources.HasFlag(BewitTokenSource.QueryString))
        {
            token = context.Request.Query[config.QueryParamName];

            logger.LogDebug(
                "BewitTokenExtraction: QueryString '{QueryParam}' value={HasValue}",
                config.QueryParamName, !string.IsNullOrWhiteSpace(token));
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            context.Items[config.ContextKey] = token;

            logger.LogDebug(
                "BewitTokenExtraction: Stored token in Items['{ContextKey}'] (length={Length})",
                config.ContextKey, token.Length);
        }
        else
        {
            logger.LogDebug(
                "BewitTokenExtraction: No token found. Request headers: {Headers}",
                string.Join(", ", context.Request.Headers.Keys));
        }

        await next(context);
    }
}
