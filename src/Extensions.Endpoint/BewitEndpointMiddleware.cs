using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Bewit.Validation;
using Bewit.Validation.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Bewit.Endpoint
{
    public class BewitEndpointMiddleware
    {

        private readonly RequestDelegate _next;

        private readonly IBewitTokenValidator<string> _tokenValidator;

        public BewitEndpointMiddleware(
            RequestDelegate next,
            IBewitTokenValidator<string> tokenValidator)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _tokenValidator = tokenValidator ??
                throw new ArgumentNullException(nameof(tokenValidator));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var endpoint = context.GetEndpoint();

            var endpointAttribute = endpoint?.Metadata.GetMetadata<BewitEndpointAttribute>();
            if (endpointAttribute == null)
            {
                await _next(context);

                return;
            }

            const string bewitQueryStringParameter = "bewit";

            string path = GetRelativeUrl(context, bewitQueryStringParameter);

            string bewitToken =
                context.Request.Query[bewitQueryStringParameter];

            if (bewitToken == null)
            {
                Unauthorize(context);

                return;
            }

            bewitToken = WebUtility.UrlDecode(bewitToken);

            string payload;

            try
            {
                payload = await _tokenValidator.ValidateBewitTokenAsync(
                    new BewitToken<string>(bewitToken),
                    context.RequestAborted);
            }
            catch (BewitException)
            {
                Unauthorize(context);

                return;
            }

            if (!string.Equals(path, payload,
                StringComparison.CurrentCultureIgnoreCase))
            {
                Unauthorize(context);

                return;
            }

            await _next(context);
        }

        private static string GetRelativeUrl(
            HttpContext context,
            string bewitQueryStringParameter)
        {
            string path = context.Request.Path.Value?.ToLowerInvariant();

            NameValueCollection newQueryString = HttpUtility.ParseQueryString(
                context.Request.QueryString.Value);
            newQueryString.Remove(bewitQueryStringParameter);

            if (newQueryString.Count != 0)
            {
                path = $"{path}?{newQueryString}";
            }

            return path;
        }

        private static void Unauthorize(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        }
    }
}
