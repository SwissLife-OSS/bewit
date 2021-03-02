using HotChocolate.Types;

namespace Bewit.Extensions.HotChocolate.Validation
{
    public static class ObjectFieldDescriptorExtensions
    {
        public static IObjectFieldDescriptor AuthorizeBewit<T>(
            this IObjectFieldDescriptor descriptor)
        {
            return descriptor
                .Directive<BewitAuthorizeDirectiveType<T>>();
        }
    }
}
