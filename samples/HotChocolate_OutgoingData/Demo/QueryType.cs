using Bewit.Extensions.HotChocolate;
using HotChocolate.Types;

namespace Demo
{
    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(
            IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Name("Query");

            descriptor.Field(t => t.GetDownloadUrl(default))
                .Type<NonNullType<StringType>>()
                .UseBewitUrlProtection();
        }
    }
}
