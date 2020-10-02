using System;
using System.Collections.Generic;
using Bewit.Core;
using Bewit.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Mvc.Filter
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            var options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitUrlAuthorizationFilter(options, build);
        }

        public static IServiceCollection AddBewitUrlAuthorizationFilter(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            services.AddBewitValidation<string>(options, build);
            services.AddTransient<BewitUrlAuthorizationAttribute>();
            return services;
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            var options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitFilter(options, build);
        }

        public static IServiceCollection AddBewitFilter(
            this IServiceCollection services,
            BewitOptions options,
            Action<BewitRegistrationBuilder> build)
        {
            services.AddBewitValidation<IDictionary<string, object>>(options, build);
            services.AddTransient<BewitAttribute>();
            return services;
        }
    }
}
