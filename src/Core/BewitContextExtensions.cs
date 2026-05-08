using Microsoft.AspNetCore.Http;

namespace Bewit;

/// <summary>
/// Extension methods for storing and retrieving bewit payloads from <see cref="HttpContext"/>.
/// Used by HotChocolate and MVC middleware to pass validated payloads to resolvers/controllers.
/// </summary>
/// <example>
/// <code>
/// // Set payload after validation (in middleware)
/// httpContextAccessor.SetBewitPayload(myPayload);
///
/// // Get payload in resolver or controller
/// var payload = httpContextAccessor.GetBewitPayload&lt;MyPayload&gt;();
/// </code>
/// </example>
public static class BewitContextExtensions
{
    private const string Key = "Bewit.Payload";

    /// <summary>
    /// Stores a validated bewit payload in the current <see cref="HttpContext"/>.
    /// </summary>
    public static void SetBewitPayload(
        this IHttpContextAccessor httpContextAccessor,
        object value)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            httpContext.Items[Key] = value;
        }
    }

    /// <summary>
    /// Retrieves a validated bewit payload from the current <see cref="HttpContext"/>.
    /// </summary>
    /// <typeparam name="T">The expected payload type.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if no payload is found or the type doesn't match.</exception>
    public static T GetBewitPayload<T>(
        this IHttpContextAccessor httpContextAccessor)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null
            && httpContext.Items.TryGetValue(Key, out var value)
            && value is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException(
            $"No bewit payload of type {typeof(T).Name} found in the current HttpContext.");
    }
}
