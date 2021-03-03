using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bewit.Validation;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Bewit.Mvc.Filter
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromBewitAttribute: Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class BewitAttribute: Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            CancellationToken cancellationToken =
                context.HttpContext.RequestAborted;

            OnActionExecutingAsync(context, cancellationToken)
                .ConfigureAwait(true).GetAwaiter()
                .GetResult();
        }

        private async Task OnActionExecutingAsync(
            ActionExecutingContext context, 
            CancellationToken cancellationToken)
        {
            List<ControllerParameterDescriptor> parameters =
                context.ActionDescriptor.Parameters
                    .OfType<ControllerParameterDescriptor>()
                    .Where(p => p.ParameterInfo
                        .CustomAttributes.Any(a =>
                            a.AttributeType == typeof(FromBewitAttribute)))
                    .ToList();

            IBewitTokenValidator<IDictionary<string, object>> tokenValidator =
                GetBewitTokenValidator(context);

            string bewitToken = GetBewitFromUrl(context);
            IDictionary<string, object> bewit = await
                tokenValidator.ValidateBewitTokenAsync(
                    new BewitToken<IDictionary<string, object>>(bewitToken),
                    cancellationToken);

            foreach (ControllerParameterDescriptor param in parameters)
            {
                string bewitParameter
                    = bewit.Keys.LastOrDefault(b =>
                        string.Equals(b, param.Name,
                            StringComparison.CurrentCultureIgnoreCase));

                if (bewitParameter != null)
                {
                    context.ActionArguments[param.Name] = bewit[bewitParameter];
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //nothing to do here
        }

        private static IBewitTokenValidator<IDictionary<string, object>> GetBewitTokenValidator(
            FilterContext context)
        {
            return context.HttpContext
                .RequestServices
                .GetService<IBewitTokenValidator<IDictionary<string, object>>>();
        }

        private static StringValues GetBewitFromUrl(
            FilterContext context)
        {
            return context.HttpContext.Request.Query["bewit"];
        }
    }
}
