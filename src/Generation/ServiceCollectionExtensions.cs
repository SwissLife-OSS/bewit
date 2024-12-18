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
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>()!;
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
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>()!;
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
            return services.AddBewitGeneration(options, registrationBuilder =>
            {
                registrationBuilder.AddPayload<TPayload>();
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
            BewitConfiguration configuration = options.Validate();

            var builder = new BewitRegistrationBuilder();
            build(builder);

            services.TryAddSingleton(options);
            services.TryAddSingleton(configuration);
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
                    context.SetCryptographyService(()
                        => new HmacSha256CryptographyService(configuration));
                }

                if (context.CreateVariablesProvider == default)
                {
                    context.SetVariablesProvider(() => new VariablesProvider());
                }

                Type typedImplementation = typeof(BewitTokenGenerator<>).MakeGenericType(context.Type);
                Type bewitTokenGenerator = typeof(IBewitTokenGenerator<>).MakeGenericType(context.Type);
                Type identifiableBewitTokenGenerator = typeof(IIdentifiableBewitTokenGenerator<>).MakeGenericType(context.Type);

                services.AddSingleton(typedImplementation, sp =>
                    ActivatorUtilities.CreateInstance(sp, typedImplementation, context));
                services.AddSingleton(bewitTokenGenerator, sp =>
                    sp.GetRequiredService(typedImplementation));
                services.AddSingleton(identifiableBewitTokenGenerator, sp =>
                    sp.GetRequiredService(typedImplementation));
            }

            return services;
        }
    }
}
