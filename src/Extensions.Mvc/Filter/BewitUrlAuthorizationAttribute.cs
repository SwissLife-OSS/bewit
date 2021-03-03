using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bewit.Validation;
using Bewit.Validation.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Mvc.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BewitUrlAuthorizationAttribute
        : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            CancellationToken cancellationToken =
                context.HttpContext.RequestAborted;

            OnAuthorizationAsync(context, cancellationToken)
                .ConfigureAwait(true).GetAwaiter()
                .GetResult();
        }

        private async Task OnAuthorizationAsync(
            AuthorizationFilterContext context, 
            CancellationToken cancellationToken)
        {
            const string bewitQueryStringParameter = "bewit";

            IBewitTokenValidator<string> tokenGenerator =
                GetBewitTokenValidator(context);

            string path = GetRelativeUrl(context, bewitQueryStringParameter);

            string bewitToken =
                context.HttpContext.Request.Query[bewitQueryStringParameter];

            if (bewitToken != null)
            {
                bewitToken = WebUtility.UrlDecode(bewitToken);

                string payload;

                try
                {
                    payload = await tokenGenerator.ValidateBewitTokenAsync(
                        new BewitToken<string>(bewitToken),
                        cancellationToken);
                }
                catch (BewitException)
                {
                    Unauthorize(context);
                    return;
                }

                if (string.Equals(path, payload,
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }
            }

            Unauthorize(context);
        }

        private static string GetRelativeUrl(
            AuthorizationFilterContext context, string bewitQueryStringParameter)
        {
            string path = context.HttpContext.Request.Path.Value
                            ?.ToLowerInvariant();

            NameValueCollection newQueryString = HttpUtility.ParseQueryString(
                context.HttpContext.Request.QueryString.Value);
            newQueryString.Remove(bewitQueryStringParameter);

            if (newQueryString.Count != 0)
            {
                path = $"{path}?{newQueryString}";
            }

            return path;
        }

        private static IBewitTokenValidator<string> GetBewitTokenValidator(
            AuthorizationFilterContext context)
        {
            return context.HttpContext
                .RequestServices
                .GetService<IBewitTokenValidator<string>>();
        }

        private static void Unauthorize(AuthorizationFilterContext context)
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
        }
    }
}
