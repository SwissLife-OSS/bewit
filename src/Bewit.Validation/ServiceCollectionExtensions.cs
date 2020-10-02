using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Validation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitValidation<TPayload>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBewitValidation<TPayload>(configuration, build => { });
        }

        public static IServiceCollection AddBewitValidation<TPayload>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitValidation<TPayload>(options, build);
        }

        public static IServiceCollection AddBewitValidation<TPayload>(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitValidation<TPayload>(options, build => { });
        }

        public static IServiceCollection AddBewitValidation<TPayload>(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            options.Validate();

            BewitRegistrationBuilder builder = new BewitRegistrationBuilder();
            build(builder);

            if (builder.GetRepository == default)
            {
                services.AddTransient<IBewitTokenValidator<TPayload>>(ctx => 
                    new BewitTokenValidator<TPayload>(
                        builder.GetCryptographyService(options),
                        new VariablesProvider()
                    ));
            }
            else
            {
                services.AddTransient<IBewitTokenValidator<TPayload>>(ctx =>
                    new PersistedBewitTokenValidator<TPayload>(
                        builder.GetCryptographyService(options),
                        new VariablesProvider(),
                        builder.GetRepository()
                    ));
            }

            return services;
        }
    }
}
