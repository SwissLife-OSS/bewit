using Bewit.Generation;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitMiddleware<T>(FieldDelegate next) where T : notnull
{
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        await next(context);

        if (context.Result is T payload)
        {
            var generator = context.Services
                .GetRequiredService<IBewitTokenGenerator<T>>();

            BewitToken<T> token = await generator.GenerateBewitTokenAsync(
                payload, null, context.RequestAborted);

            context.Result = (string)token;
        }
    }
}
