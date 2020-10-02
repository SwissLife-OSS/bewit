using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Execution;
using HotChocolate.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Bewit.Extensions.HotChocolate
{
    public class BewitTokenHeaderInterceptor
        : IQueryRequestInterceptor<HttpContext>
    {
        public Task OnCreateAsync(
            HttpContext context,
            IQueryRequestBuilder requestBuilder,
            CancellationToken cancellationToken)
        {
            if (context.Request.Headers.TryGetValue(
                    BewitTokenHeader.Value,
                    out StringValues bewitToken))
            {
                requestBuilder.AddProperty(BewitTokenHeader.Value, bewitToken.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
