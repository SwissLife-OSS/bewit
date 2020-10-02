using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Generation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBewitGeneration<TPayload>(configuration, build => { });
        }

        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitGeneration<TPayload>(options, build);
        }

        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitGeneration<TPayload>(options, build => { });
        }

        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            options.Validate();

            BewitRegistrationBuilder builder = new BewitRegistrationBuilder();
            build(builder);

            if (builder.GetRepository == default)
            {
                services.AddTransient<IBewitTokenGenerator<TPayload>>(ctx =>
                    new BewitTokenGenerator<TPayload>(
                        options.TokenDuration,
                        builder.GetCryptographyService(options),
                        new VariablesProvider()
                    ));
            }
            else
            {
                services.AddTransient<IBewitTokenGenerator<TPayload>>(ctx =>
                    new PersistedBewitTokenGenerator<TPayload>(
                        options.TokenDuration,
                        builder.GetCryptographyService(options),
                        new VariablesProvider(),
                        builder.GetRepository()
                    ));
            }

            return services;
        }
    }
}
