using System;
using Microsoft.AspNetCore.Http;

namespace Bewit
{
    public static class BewitContextExtensions
    {
        private static readonly string _key = nameof(BewitContext);

        public static void SetBewitPayload(
            this IHttpContextAccessor httpContextAccessor,
            object value)
        {
            HttpContext httpContext = httpContextAccessor?.HttpContext;

            if (httpContext != null)
            {
                httpContext.Items[_key] = new BewitContext(value);
            }
        }

        public static T GetBewitPayload<T>(
            this IHttpContextAccessor httpContextAccessor)
            where T : class
        {
            return httpContextAccessor.GetBewitContext().Get<T>();
        }

        private static BewitContext GetBewitContext(
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
