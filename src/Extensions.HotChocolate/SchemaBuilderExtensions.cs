using System;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate
{
    public static class SchemaBuilderExtensions
    {
        public static IRequestExecutorBuilder AddBewitAuthorizeDirectiveType(
          this IRequestExecutorBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            return builder.ConfigureSchema(b => b.AddBewitAuthorizeDirectiveType());
        }

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
