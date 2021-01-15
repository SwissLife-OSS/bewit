using Host.Models;
using HotChocolate.Types;

namespace Host.Types
{
    public class DocumentType: ObjectType<Document>
    {
        protected override void Configure(IObjectTypeDescriptor<Document> descriptor)
        {
            descriptor.Field(d => d.Content).Ignore();
        }
    }
}
