using System;
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

        public static IServiceCollection AddBewitValidation<TPayload>(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitValidation(options, builder =>
            {
                builder.AddPayload<TPayload>();
            });
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

                Type implementation = typeof(BewitTokenValidator<>);
                Type typedImplementation = implementation.MakeGenericType(context.Type);
                Type service = typeof(IBewitTokenValidator<>);
                Type typedService = service.MakeGenericType(context.Type);

                services.AddSingleton(typedService, sp => ActivatorUtilities
                    .CreateInstance(sp, typedImplementation, context));
            }

            return services;
        }
    }
}
