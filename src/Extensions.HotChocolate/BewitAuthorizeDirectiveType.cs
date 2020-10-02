using HotChocolate.Types;

namespace Bewit.Extensions.HotChocolate
{
    public class BewitAuthorizeDirectiveType
        : DirectiveType
    {
        protected override void Configure(
            IDirectiveTypeDescriptor descriptor)
        {
            descriptor
                .Name("authorizeBewitToken")
                .Location(DirectiveLocation.FieldDefinition)
                .Use<BewitAuthorizationMiddleware>();
        }
    }
}
