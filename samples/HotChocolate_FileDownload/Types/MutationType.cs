using Bewit.Extensions.HotChocolate.Generation;
using HotChocolate.Types;

namespace Host.Types
{
    public class MutationType
        : ObjectType<Mutation>
    {
        protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor)
        {
            descriptor.Field(t => t.CreateDownloadLink(default!))
                // Sign the returned url with bewit
                .UseBewitUrlProtection();
        }
    }
}
