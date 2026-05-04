using Bewit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitServiceCollectionExtensions
{
    public static IServiceCollection AddBewit(
        this IServiceCollection services,
        Action<BewitBuilder> configure)
    {
        var builder = new BewitBuilder(services);
        configure(builder);

        services.TryAddSingleton<IVariablesProvider, VariablesProvider>();
        services.AddHttpContextAccessor();

        foreach (Action<IServiceCollection> registration in builder.PayloadRegistrations)
        {
            registration(services);
        }

        return services;
    }
}
