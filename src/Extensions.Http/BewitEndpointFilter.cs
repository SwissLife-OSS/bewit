using Bewit.Exceptions;
using Bewit.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.Http;

public sealed class BewitEndpointFilter<T> : IEndpointFilter
    where T : notnull
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var options = httpContext.RequestServices
            .GetRequiredService<IOptions<BewitTokenExtractionOptions>>();

        BewitTokenExtractionOptions config = options.Value;

        string? tokenString = null;

        if (httpContext.Items.TryGetValue(config.ContextKey, out var tokenObj))
        {
            tokenString = tokenObj as string;
        }

        if (string.IsNullOrWhiteSpace(tokenString)
            && httpContext.Request.Headers.TryGetValue(
                config.HeaderName, out var headerValues)
            && headerValues.Count > 0)
        {
            tokenString = headerValues[0];
        }

        if (string.IsNullOrWhiteSpace(tokenString))
        {
            tokenString = httpContext.Request.Query[config.QueryParamName];
        }

        if (string.IsNullOrWhiteSpace(tokenString))
        {
            return Results.Unauthorized();
        }

        try
        {
            var validator = httpContext.RequestServices
                .GetRequiredService<IBewitTokenValidator<T>>();

            var payload = await validator.ValidateBewitTokenAsync(
                new BewitToken<T>(tokenString),
                httpContext.RequestAborted);

            var httpContextAccessor = httpContext.RequestServices
                .GetRequiredService<IHttpContextAccessor>();

            httpContextAccessor.SetBewitPayload(payload);
        }
        catch (BewitException)
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
