using HotChocolate.Types;

namespace Bewit.Extensions.HotChocolate.Generation
{
    public static class ObjectFieldDescriptorExtensions
    {
        public static IObjectFieldDescriptor UseBewitProtection<TPayload>(
            this IObjectFieldDescriptor descriptor)
        {
            return descriptor
                .Use<BewitMiddleware<TPayload>>()
                .Type<NonNullType<StringType>>();
        }
        public static IObjectFieldDescriptor UseBewitUrlProtection(
            this IObjectFieldDescriptor descriptor)
        {
            return descriptor.Use<BewitUrlMiddleware>()
                .Type<NonNullType<StringType>>();
        }
    }
}
