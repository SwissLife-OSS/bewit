using HotChocolate.Types;

namespace Host.Types
{
    public class MutationType : ObjectType<Mutation>
    {
        protected override void Configure(
            IObjectTypeDescriptor<Mutation> descriptor)
        {
            descriptor
                .Field(q => q.CreateBewitToken(default!));
        }
    }
}
