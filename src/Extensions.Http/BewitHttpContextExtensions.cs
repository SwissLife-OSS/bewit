using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Bewit.AspNetCore;

public static class BewitHttpContextExtensions
{
    private static string PayloadKey<TPayload>() => $"Bewit.Payload:{typeof(TPayload).AssemblyQualifiedName}";

    public static void SetBewitPayload<TPayload>(this HttpContext context, TPayload payload)
        where TPayload : notnull => context.Items[PayloadKey<TPayload>()] = payload;

    public static TPayload GetBewitPayload<TPayload>(this HttpContext context)
        where TPayload : notnull =>
        context.Items.TryGetValue(PayloadKey<TPayload>(), out object? value) && value is TPayload payload
            ? payload
            : throw new InvalidOperationException(
                $"No validated bewit payload of type '{typeof(TPayload)}' is available.");

    public static string? GetBewitToken(this HttpContext context, BewitAspNetCoreOptions options)
    {
        if (options.Sources.HasFlag(BewitTokenSource.Header)
            && context.Request.Headers.TryGetValue(options.HeaderName, out StringValues values)
            && values.Count > 0
            && !string.IsNullOrWhiteSpace(values[0]))
        {
            return values[0];
        }
        if (options.Sources.HasFlag(BewitTokenSource.QueryString)
            && context.Request.Query.TryGetValue(options.QueryParameterName, out values)
            && values.Count > 0
            && !string.IsNullOrWhiteSpace(values[0]))
        {
            return values[0];
        }
        return null;
    }
}
