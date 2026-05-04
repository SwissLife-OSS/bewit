using System.Collections.Specialized;
using System.Web;
using Bewit.Exceptions;
using Bewit.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.Extensions.Mvc;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class BewitUrlAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<BewitTokenExtractionOptions>>();

        string queryParamName = options.Value.QueryParamName;

        string? bewitToken = null;

        if (context.HttpContext.Request.Headers.TryGetValue(
                options.Value.HeaderName, out var headerValues)
            && headerValues.Count > 0)
        {
            bewitToken = headerValues[0];
        }

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            bewitToken = context.HttpContext.Request.Query[queryParamName];
        }

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            context.Result = new StatusCodeResult(403);

            return;
        }

        var validator = context.HttpContext.RequestServices
            .GetRequiredService<IBewitTokenValidator<string>>();

        string payload;

        try
        {
            payload = await validator.ValidateBewitTokenAsync(
                new BewitToken<string>(bewitToken),
                context.HttpContext.RequestAborted);
        }
        catch (BewitException)
        {
            context.Result = new StatusCodeResult(403);

            return;
        }

        string path = GetRelativeUrl(context, queryParamName);

        if (!string.Equals(path, payload, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new StatusCodeResult(403);
        }
    }

    private static string GetRelativeUrl(
        AuthorizationFilterContext context,
        string queryParamName)
    {
        string? path = context.HttpContext.Request.Path.Value?.ToLowerInvariant();

        NameValueCollection queryString = HttpUtility.ParseQueryString(
            context.HttpContext.Request.QueryString.Value ?? string.Empty);

        queryString.Remove(queryParamName);

        if (queryString.Count != 0)
        {
            return $"{path}?{queryString}";
        }

        return path ?? string.Empty;
    }
}
