using System;
using Bewit.Core;
using Bewit.Validation;
using HotChocolate.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.HotChocolate
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitAuthorization(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services.AddBewitAuthorization(configuration, build => { });
        }

        public static IServiceCollection AddBewitAuthorization(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<BewitRegistrationBuilder> build)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();

            return services
                .AddSingleton<IBewitContext, BewitContext>()
                .AddBewitValidation<object>(options, build)
                .AddSingleton<IQueryRequestInterceptor<HttpContext>, BewitTokenHeaderInterceptor>();
        }
    }
}
