using System;
using HotChocolate;

namespace Bewit.HotChocolate
{
    public static class SchemaBuilderExtensions
    {
        public static ISchemaBuilder AddBewitAuthorizeDirectiveType(
            this ISchemaBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddDirectiveType<BewitAuthorizeDirectiveType>();
        }
    }
}
