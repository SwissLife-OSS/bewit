using System;
using Bewit.Validation;
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
                .AddBewitValidation(options, build);

            return builder;
        }
    }
}
