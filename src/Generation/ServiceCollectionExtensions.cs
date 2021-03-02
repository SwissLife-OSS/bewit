using System;
using Bewit.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBewitGeneration(configuration, build => build.AddPayload<TPayload>());
        }

        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitGeneration(options, registrationBuilder =>
            {
                registrationBuilder.AddPayload<TPayload>();
                build(registrationBuilder);
            });
        }

        public static IServiceCollection AddBewitGeneration<TPayload>(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitGeneration(options, build => build.AddPayload<TPayload>());
        }

        public static IServiceCollection AddBewitGeneration(
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
                Type implementation = typeof(BewitTokenGenerator<>);
                Type typedImplementation = implementation.MakeGenericType(payloadBuilder.Type);
                Type service = typeof(IBewitTokenGenerator<>);
                Type typedService = service.MakeGenericType(payloadBuilder.Type);
                services.AddTransient(typedService, typedImplementation);
            }

            return services;
        }
    }
}
