using Bewit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitServiceCollectionExtensions
{
    public static IServiceCollection AddBewit(
        this IServiceCollection services,
        Action<BewitBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new BewitBuilder(services);
        configure(builder);

        OptionsBuilder<BewitOptions> optionsBuilder = services.AddOptions<BewitOptions>();
        if (builder.ConfigurationSection is not null)
        {
            optionsBuilder.BindConfiguration(builder.ConfigurationSection);
        }
        if (builder.ConfigureAction is not null)
        {
            optionsBuilder.Configure(builder.ConfigureAction);
        }
        optionsBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<BewitOptions>, BewitOptionsValidator>());
        optionsBuilder.ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IBewitTokenIdGenerator, BewitTokenIdGenerator>();

        foreach (Action<IServiceCollection> registration in builder.Registrations)
        {
            registration(services);
        }

        return services;
    }
}
