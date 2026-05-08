using Bewit.Generation;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitUrlMiddleware(FieldDelegate next)
{
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        await next(context);

        if (context.Result is string url && !string.IsNullOrWhiteSpace(url))
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            string pathAndQuery = uri.IsAbsoluteUri
                ? uri.PathAndQuery
                : url;

            var generator = context.Services
                .GetRequiredService<IBewitTokenGenerator<string>>();

            BewitToken<string> token = await generator.GenerateBewitTokenAsync(
                pathAndQuery, null, context.RequestAborted);

            string encodedToken = Uri.EscapeDataString((string)token);

            string separator = url.Contains('?') ? "&" : "?";

            context.Result = $"{url}{separator}bewit={encodedToken}";
        }
    }
}
