using System;
using Microsoft.AspNetCore.Http;

namespace Bewit
{
    public static class BewitContextExtensions
    {
        private static readonly string _key = nameof(BewitContext);

        public static void SetBewitContext(
            this IHttpContextAccessor httpContextAccessor,
            object value)
        {
            HttpContext httpContext = httpContextAccessor?.HttpContext;

            if (httpContext != null)
            {
                httpContext.Items[_key] = new BewitContext(value);
            }
        }

        public static BewitContext GetBewitContext(
            this IHttpContextAccessor httpContextAccessor)
        {
            HttpContext httpContext = httpContextAccessor?.HttpContext;

            if (httpContext != null &&
                httpContext.Items.TryGetValue(_key, out var context) &&
                context is BewitContext bewitContext)
            {
                return bewitContext;
            }

            throw new InvalidOperationException($"No {_key} found");
        }
    }
}
