using HotChocolate.Types;

namespace Bewit.Extensions.HotChocolate;

public static class BewitObjectFieldDescriptorExtensions
{
    public static IObjectFieldDescriptor AuthorizeBewit<T>(
        this IObjectFieldDescriptor descriptor)
        where T : notnull
    {
        return descriptor.Use<BewitAuthorizationMiddleware<T>>();
    }

    public static IObjectFieldDescriptor UseBewitProtection<T>(
        this IObjectFieldDescriptor descriptor)
        where T : notnull
    {
        return descriptor.Use<BewitMiddleware<T>>();
    }

    public static IObjectFieldDescriptor UseBewitUrlProtection(
        this IObjectFieldDescriptor descriptor)
    {
        return descriptor.Use<BewitUrlMiddleware>();
    }
}
