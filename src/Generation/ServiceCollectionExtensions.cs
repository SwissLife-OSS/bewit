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

            foreach (BewitPayload payloadBuilder in builder.Payloads)
            {
                Type generator = typeof(BewitTokenGenerator<>);
                Type typedGenerator = generator.MakeGenericType(payloadBuilder.Type);
                services.AddTransient(typeof(IBewitTokenGenerator<>), typedGenerator);
            }

            return services;
        }
    }
}
