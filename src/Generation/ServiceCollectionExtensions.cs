using System;
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
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            return services.AddBewitGeneration(options, registrationBuilder =>
            {
                registrationBuilder.AddPayload<TPayload>();
                build(registrationBuilder);
            });
        }

        public static IServiceCollection AddBewitGeneration(
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
            services.TryAddSingleton<INonceRepository, DefaultNonceRepository>();

            foreach (BewitPayloadContext context in builder.Payloads)
            {
                if (context.CreateRepository == default)
                {
                    context.SetRepository(() => new DefaultNonceRepository());
                }

                if (context.CreateCryptographyService == default)
                {
                    context.SetCryptographyService(() => new HmacSha256CryptographyService(options));
                }

                if (context.CreateVariablesProvider == default)
                {
                    context.SetVariablesProvider(() => new VariablesProvider());
                }

                Type implementation = typeof(BewitTokenGenerator<>);
                Type typedImplementation = implementation.MakeGenericType(context.Type);
                Type service = typeof(IBewitTokenGenerator<>);
                Type typedService = service.MakeGenericType(context.Type);

                services.AddSingleton(typedService, sp => ActivatorUtilities
                    .CreateInstance(sp, typedImplementation, context));
            }

            return services;
        }
    }
}
