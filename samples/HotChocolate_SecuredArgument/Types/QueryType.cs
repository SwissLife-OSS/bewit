using HotChocolate.Types;

namespace Host.Types
{
    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(
            IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field(q => q.GetSecretDocument(default!)).Ignore();
                /*.AuthorizeBewit()*/;
        }
    }
}
