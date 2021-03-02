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
            
            var builder = new BewitRegistrationBuilder(services);
            build(builder);

            services.TryAddSingleton(options);
            services.TryAddSingleton<ICryptographyService, HmacSha256CryptographyService>();
            services.TryAddSingleton<IVariablesProvider, VariablesProvider>();
            services.TryAddSingleton<INonceRepository, MemoryNonceRepository>();

            foreach (BewitPayload payloadBuilder in builder.Payloads)
            {
                Type implementation = typeof(BewitTokenValidator<>);
                Type typedImplementation = implementation.MakeGenericType(payloadBuilder.Type);
                Type service = typeof(IBewitTokenValidator<>);
                Type typedService = service.MakeGenericType(payloadBuilder.Type);
                services.AddTransient(typedService, typedImplementation);
            }

            return services;
        }
    }
}
