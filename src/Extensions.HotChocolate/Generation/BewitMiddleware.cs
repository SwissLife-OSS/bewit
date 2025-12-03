using System;
using System.Threading.Tasks;
using Bewit.Generation;
using HotChocolate.Resolvers;

namespace Bewit.Extensions.HotChocolate.Generation
{
    public class BewitMiddleware<TPayload>
    {
        private readonly FieldDelegate _next;

        public BewitMiddleware(FieldDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(
            IMiddlewareContext context,
            IBewitTokenGenerator<TPayload> tokenGenerator)
        {
            await _next(context).ConfigureAwait(false);

            if (context.Result is TPayload result)
            {
                BewitToken<TPayload> bewit
                    = await tokenGenerator.GenerateBewitTokenAsync(
                        result, context.GetBewitTokenExtraProperties(), context.RequestAborted);

                context.Result = (string)bewit;
            }
        }
    }
}
