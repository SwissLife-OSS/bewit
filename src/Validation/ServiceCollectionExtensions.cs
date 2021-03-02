using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Validation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitValidation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBewitValidation(configuration, build => { });
        }

        public static IServiceCollection AddBewitValidation(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitValidation(options, build);
        }

        public static IServiceCollection AddBewitValidation(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitValidation(options, build => { });
        }

        public static IServiceCollection AddBewitValidation(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            options.Validate();

            BewitRegistrationBuilder builder = new BewitRegistrationBuilder();
            build(builder);

            foreach (BewitPayloadBuilder payloadBuilder in builder.PayloadBuilders)
            {
                if (payloadBuilder.CreateRepository == default)
                {
                    services.AddTransient(
                        typeof(IBewitTokenValidator<>),
                        serviceProvider => ActivatorUtilities.CreateInstance(
                            serviceProvider,
                            typeof(BewitTokenValidator<>),
                            builder.GetCryptographyService(options),
                            new VariablesProvider()));
                }
                else
                {
                    services.AddTransient(
                        typeof(IBewitTokenValidator<>),
                        serviceProvider => ActivatorUtilities.CreateInstance(
                            serviceProvider,
                            typeof(PersistedBewitTokenValidator<>),
                            builder.GetCryptographyService(options),
                            new VariablesProvider(),
                            payloadBuilder.CreateRepository()));
                }
            }

            return services;
        }
    }
}
