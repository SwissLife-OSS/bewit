using System;
using System.Collections.Generic;
using Bewit.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Mvc.Filter
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitUrlAuthorizationFilter(options, rb => { }, pb => { });
        }

        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitPayloadContext> payloadBuilder)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitUrlAuthorizationFilter(options, rb => { }, payloadBuilder);
        }

        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitPayloadContext> payloadBuilder)
        {
            return services.AddBewitUrlAuthorizationFilter(options, rb => { }, payloadBuilder);
        }

        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> registrationBuilder,
            Action<BewitPayloadContext> payloadBuilder)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitUrlAuthorizationFilter(options, registrationBuilder, payloadBuilder);
        }

        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> registrationBuilder,
            Action<BewitPayloadContext> payloadBuilder)
        {
            services.AddBewitValidation(options, b =>
            {
                payloadBuilder(b.AddPayload<string>());
                registrationBuilder(b);
            });
            services.AddTransient<BewitUrlAuthorizationAttribute>();
            return services;
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitFilter(options, rb => { }, pb => { });
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitPayloadContext> payloadBuilder)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitFilter(options, rb => { }, payloadBuilder);
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitPayloadContext> payloadBuilder)
        {
            return services.AddBewitFilter(options, rb => { }, payloadBuilder);
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> registrationBuilder,
            Action<BewitPayloadContext> payloadBuilder)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitFilter(options, registrationBuilder, payloadBuilder);
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> registrationBuilder,
            Action<BewitPayloadContext> payloadBuilder)
        {
            services.AddBewitValidation(options, b =>
            {
                payloadBuilder(b.AddPayload<IDictionary<string, object>>());
                registrationBuilder(b);
            });
            services.AddTransient<BewitAttribute>();
            return services;
        }
    }
}
