using Bewit.Validation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.Extensions.Mvc;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class BewitMvcAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var options = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<BewitTokenExtractionOptions>>();

        BewitTokenExtractionOptions config = options.Value;

        string? bewitToken = null;

        if (context.HttpContext.Request.Headers.TryGetValue(
                config.HeaderName, out var headerValues)
            && headerValues.Count > 0)
        {
            bewitToken = headerValues[0];
        }

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            bewitToken = context.HttpContext.Request.Query[config.QueryParamName];
        }

        if (string.IsNullOrWhiteSpace(bewitToken))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(401);

            return;
        }

        var validator = context.HttpContext.RequestServices
            .GetRequiredService<IBewitTokenValidator<IDictionary<string, object>>>();

        IDictionary<string, object> payload = await validator
            .ValidateBewitTokenAsync(
                new BewitToken<IDictionary<string, object>>(bewitToken),
                context.HttpContext.RequestAborted);

        List<ControllerParameterDescriptor> parameters = context.ActionDescriptor
            .Parameters
            .OfType<ControllerParameterDescriptor>()
            .Where(p => p.ParameterInfo.CustomAttributes
                .Any(a => a.AttributeType == typeof(FromBewitAttribute)))
            .ToList();

        foreach (ControllerParameterDescriptor param in parameters)
        {
            string? key = payload.Keys
                .LastOrDefault(k => string.Equals(
                    k, param.Name, StringComparison.OrdinalIgnoreCase));

            if (key is not null)
            {
                context.ActionArguments[param.Name] = payload[key];
            }
        }

        await next();
    }
}
