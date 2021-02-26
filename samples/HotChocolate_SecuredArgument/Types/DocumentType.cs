using Bewit.Extensions.HotChocolate.Generation;
using Host.Models;
using HotChocolate.Types;

namespace Host.Types
{
    public class DocumentType: ObjectType<Document>
    {
        protected override void Configure(IObjectTypeDescriptor<Document> descriptor)
        {
            descriptor.Field("bewit")
                .Resolver(ctx => ctx.Parent<Document>().Name)
                .UseBewitProtection<string>();
        }
    }
}
