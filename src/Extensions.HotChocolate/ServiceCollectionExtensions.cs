using System;
using Bewit.Core;
using Bewit.Validation;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate
{
    public static class ServiceCollectionExtensions
    {
        public static IRequestExecutorBuilder AddBewitAuthorization(
            this IRequestExecutorBuilder builder,
            IConfiguration configuration)
        {
            return builder.AddBewitAuthorization(configuration, build => { });
        }

        public static IRequestExecutorBuilder AddBewitAuthorization(
            this IRequestExecutorBuilder builder,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();

            builder
                .AddHttpRequestInterceptor<BewitTokenHeaderInterceptor>()
                .Services
                .AddSingleton<IBewitContext, BewitContext>()
                .AddBewitValidation<object>(options, build);

            return builder;
        }
    }
}
