using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore.Utilities;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Bewit.Extensions.HotChocolate
{
    public class BewitTokenHeaderInterceptor
        : IHttpRequestInterceptor
    {
        public ValueTask OnCreateAsync(
            HttpContext context,
            IRequestExecutor requestExecutor,
            IQueryRequestBuilder requestBuilder,
            CancellationToken cancellationToken)
        {
            if (context.Request.Headers.TryGetValue(
                BewitTokenHeader.Value,
                out StringValues bewitToken))
            {
                requestBuilder.AddProperty(BewitTokenHeader.Value, bewitToken.ToString());
            }

            return default;
        }
    }
}
