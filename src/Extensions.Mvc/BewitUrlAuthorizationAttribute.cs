using System.Collections.Specialized;
using System.Web;
using Bewit.Exceptions;
using Bewit.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.Mvc;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class BewitUrlAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string BewitQueryParam = "bewit";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        string? bewitToken = context.HttpContext.Request.Query[BewitQueryParam];

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

        string path = GetRelativeUrl(context);

        if (!string.Equals(path, payload, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new StatusCodeResult(403);
        }
    }

    private static string GetRelativeUrl(AuthorizationFilterContext context)
    {
        string? path = context.HttpContext.Request.Path.Value?.ToLowerInvariant();

        NameValueCollection queryString = HttpUtility.ParseQueryString(
            context.HttpContext.Request.QueryString.Value ?? string.Empty);

        queryString.Remove(BewitQueryParam);

        if (queryString.Count != 0)
        {
            return $"{path}?{queryString}";
        }

        return path ?? string.Empty;
    }
}
