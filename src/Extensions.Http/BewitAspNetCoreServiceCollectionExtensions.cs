using Bewit.AspNetCore;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitAspNetCoreServiceCollectionExtensions
{
    public static IServiceCollection AddBewitAspNetCore(
        this IServiceCollection services,
        Action<BewitAspNetCoreOptions>? configure = null)
    {
        OptionsBuilder<BewitAspNetCoreOptions> options = services.AddOptions<BewitAspNetCoreOptions>();
        if (configure is not null)
            options.Configure(configure);
        options.Validate(value => !string.IsNullOrWhiteSpace(value.HeaderName), "HeaderName is required.")
            .Validate(value => !string.IsNullOrWhiteSpace(value.QueryParameterName), "QueryParameterName is required.")
            .ValidateOnStart();
        return services;
    }
}
