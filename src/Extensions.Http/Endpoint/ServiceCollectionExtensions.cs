using Bewit.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Http.Endpoint
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBewitEndpointAuthorization(
            this IServiceCollection services,
            BewitOptions options)
        {
            return services.AddBewitValidation<string>(options);
        }
        public static IServiceCollection AddBewitEndpointAuthorization(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            BewitOptions options = configuration.GetSection("Bewit").Get<BewitOptions>();
            return services.AddBewitEndpointAuthorization(options);
        }
    }
}
