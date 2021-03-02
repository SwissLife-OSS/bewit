using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Generation
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitGeneration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBewitGeneration(configuration, build => { });
        }

        public static IServiceCollection AddBewitGeneration(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitGeneration(options, build);
        }

        public static IServiceCollection AddBewitGeneration(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitGeneration(options, build => { });
        }

        public static IServiceCollection AddBewitGeneration(
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
                        typeof(IBewitTokenGenerator<>),
                        serviceProvider => ActivatorUtilities.CreateInstance(
                            serviceProvider,
                            typeof(BewitTokenGenerator<>),
                            options.TokenDuration,
                            builder.GetCryptographyService(options),
                            new VariablesProvider()));
                }
                else
                {
                    services.AddTransient(
                        typeof(IBewitTokenGenerator<>),
                        serviceProvider => ActivatorUtilities.CreateInstance(
                            serviceProvider,
                            typeof(PersistedBewitTokenGenerator<>),
                            options.TokenDuration,
                            builder.GetCryptographyService(options),
                            new VariablesProvider(),
                            payloadBuilder.CreateRepository()));
                }
            }

            return services;
        }
    }
}
