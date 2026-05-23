using Bewit.Generation;
using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit.Extensions.HotChocolate;

internal sealed class BewitUrlMiddleware(FieldDelegate next)
{
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        IOptionsMonitor<BewitOptions> optionsMonitor =
            context.RequestServices.GetRequiredService<IOptionsMonitor<BewitOptions>>();
        BewitOptions bewitOptions = optionsMonitor.Get(typeof(string).FullName);

        BewitTokenOptions? options = null;

        if (bewitOptions.ExpiryMode == ExpiryMode.ServerControlled)
        {
            options = new();

            context.GetOrSetScopedState("bewit-extra-properties", (string key) => options);
        }

        await next(context);

        if (context.Result is string url && !string.IsNullOrWhiteSpace(url))
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            string pathAndQuery = uri.IsAbsoluteUri
                ? uri.PathAndQuery
                : url;

            IBewitTokenGenerator<string> generator = context.Services
                .GetRequiredService<IBewitTokenGenerator<string>>();

            BewitToken<string> token = await generator.GenerateBewitTokenAsync(
                pathAndQuery, options, context.RequestAborted);

            string encodedToken = Uri.EscapeDataString((string)token);

            string separator = url.Contains('?') ? "&" : "?";

            context.Result = $"{url}{separator}bewit={encodedToken}";
        }
    }
}
