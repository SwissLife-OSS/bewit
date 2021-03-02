using System;
using Bewit.Core;
using Bewit.Validation;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate.Validation
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

        public static IRequestExecutorBuilder UseBewitAuthorization(
            this IRequestExecutorBuilder builder,
            BewitOptions options)
        {
            return builder.UseBewitAuthorization(options, build => { });
        }

        public static IRequestExecutorBuilder UseBewitAuthorization(
            this IRequestExecutorBuilder builder,
            IConfiguration configuration)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();

            return builder.UseBewitAuthorization(options, build => { });
        }

        public static IRequestExecutorBuilder UseBewitAuthorization(
            this IRequestExecutorBuilder builder,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            builder
                .AddBewitAuthorizeDirectiveType()
                .UseRequest<BewitTokenHeaderRequestMiddleware>()
                .Services
                .AddSingleton<IBewitContext, BewitContext>()
                .AddBewitValidation(options, build);

            return builder;
        }
    }
}
