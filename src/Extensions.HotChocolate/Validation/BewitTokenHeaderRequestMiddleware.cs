using System.Threading.Tasks;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RequestDelegate = HotChocolate.Execution.RequestDelegate;

namespace Bewit.Extensions.HotChocolate.Validation
{
    public class BewitTokenHeaderRequestMiddleware
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RequestDelegate _next;

        public BewitTokenHeaderRequestMiddleware(
            IHttpContextAccessor httpContextAccessor,
            RequestDelegate next)
        {
            _httpContextAccessor = httpContextAccessor;
            _next = next;
        }

        public ValueTask InvokeAsync(IRequestContext requestContext)
        {
            HttpContext context = _httpContextAccessor.HttpContext;

            if (context != null &&
                context.Request.Headers
                    .TryGetValue(BewitTokenHeader.Value, out StringValues bewitToken))
            {
                requestContext.ContextData[BewitTokenHeader.Value] = bewitToken.ToString();
            }

            return _next(requestContext);
        }
    }
}
