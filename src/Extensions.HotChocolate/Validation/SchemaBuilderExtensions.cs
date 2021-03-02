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
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return builder.UseBewitAuthorization(options, build);
        }

        public static IRequestExecutorBuilder UseBewitAuthorization(
            this IRequestExecutorBuilder builder,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            builder
                .UseRequest<BewitTokenHeaderRequestMiddleware>()
                .Services
                .AddSingleton<IBewitContext, BewitContext>()
                .AddBewitValidation(options, registrationBuilder =>
                {
                    build(registrationBuilder);

                    foreach (BewitPayloadContext context in registrationBuilder.Payloads)
                    {
                        Type implementation = typeof(BewitAuthorizeDirectiveType<>);
                        Type typedImplementation = implementation.MakeGenericType(context.Type);

                        builder.ConfigureSchema(b => b.AddDirectiveType(typedImplementation));
                    }
                });

            return builder;
        }
    }
}
