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
        public static IRequestExecutorBuilder AddBewitAuthorizeDirectiveType<T>(
          this IRequestExecutorBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            return builder.ConfigureSchema(b => b.AddBewitAuthorizeDirectiveType<T>());
        }

        public static ISchemaBuilder AddBewitAuthorizeDirectiveType<T>(
            this ISchemaBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddDirectiveType<BewitAuthorizeDirectiveType<T>>();
        }

        public static IRequestExecutorBuilder UseBewitAuthorization<T>(
            this IRequestExecutorBuilder builder,
            BewitOptions options)
        {
            return builder.UseBewitAuthorization<T>(options, build => { });
        }

        public static IRequestExecutorBuilder UseBewitAuthorization<T>(
            this IRequestExecutorBuilder builder,
            IConfiguration configuration)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();

            return builder.UseBewitAuthorization<T>(options, build => { });
        }

        public static IRequestExecutorBuilder UseBewitAuthorization<T>(
            this IRequestExecutorBuilder builder,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            builder
                .AddBewitAuthorizeDirectiveType<T>()
                .UseRequest<BewitTokenHeaderRequestMiddleware>()
                .Services
                .AddSingleton<IBewitContext, BewitContext>()
                .AddBewitValidation(options, bewitRegistrationBuilder =>
                {
                    bewitRegistrationBuilder.AddPayload<T>();
                    build(bewitRegistrationBuilder);
                });

            return builder;
        }
    }
}
