using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            
            var builder = new BewitRegistrationBuilder();
            build(builder);

            services.TryAddSingleton(options);
            services.TryAddSingleton<ICryptographyService, HmacSha256CryptographyService>();
            services.TryAddSingleton<IVariablesProvider, VariablesProvider>();

            foreach (BewitPayloadContext payloadBuilder in builder.Payloads)
            {
                if (payloadBuilder.CreateRepository == default)
                {
                    payloadBuilder.SetRepository(() => new DefaultNonceRepository());
                }

                if (payloadBuilder.CreateCryptographyService == default)
                {
                    payloadBuilder.SetCryptographyService(() => new HmacSha256CryptographyService(options));
                }

                if (payloadBuilder.CreateVariablesProvider == default)
                {
                    payloadBuilder.SetVariablesProvider(() => new VariablesProvider());
                }

                Type implementation = typeof(BewitTokenValidator<>);
                Type typedImplementation = implementation.MakeGenericType(payloadBuilder.Type);
                Type service = typeof(IBewitTokenValidator<>);
                Type typedService = service.MakeGenericType(payloadBuilder.Type);

                services.AddSingleton(typedService, sp => ActivatorUtilities
                    .CreateInstance(sp, typedImplementation, payloadBuilder));
            }

            return services;
        }
    }
}
