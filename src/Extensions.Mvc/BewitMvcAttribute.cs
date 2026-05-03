using Bewit.Validation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.Mvc;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class BewitMvcAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        string? bewitToken = context.HttpContext.Request.Query["bewit"];

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
