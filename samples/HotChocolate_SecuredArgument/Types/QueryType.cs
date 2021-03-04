using Bewit.Extensions.HotChocolate.Validation;
using HotChocolate.Types;

namespace Host.Types
{
    public class QueryType : ObjectType<Query>
    {
        protected override void Configure(
            IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor
                .Field(q => q.GetSecretDocumentWithStatelessBewit(default!))
                .AuthorizeBewit<FooPayload>();

            descriptor
                .Field(q => q.GetSecretDocumentWithStatefulBewit(default!))
                .AuthorizeBewit<BarPayload>();
        }
    }
}
