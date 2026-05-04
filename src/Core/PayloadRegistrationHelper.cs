using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit;

internal static class PayloadRegistrationHelper
{
    public static void Register<T>(
        IServiceCollection services,
        PayloadBuilder<T> builder,
        string? globalConfigurationSection,
        Action<BewitOptions>? globalOptions,
        Func<IServiceProvider, INonceRepository>? builderNonceRepositoryFactory)
        where T : notnull
    {
        string optionsName = typeof(T).FullName ?? typeof(T).Name;

        var optionsBuilder = services.AddOptions<BewitOptions>(optionsName);

        string? section = builder.ConfigurationSection ?? globalConfigurationSection;

        if (section is not null)
        {
            optionsBuilder.BindConfiguration(section);
        }

        optionsBuilder
            .Configure(o =>
            {
                globalOptions?.Invoke(o);
                builder.OptionsOverride?.Invoke(o);
            })
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<BewitOptions>>(
            new BewitOptionsValidator());

        Func<IServiceProvider, INonceRepository>? factory =
            builder.NonceRepositoryFactory ?? builderNonceRepositoryFactory;

        bool hasRealNonceRepo = factory is not null;

        if (hasRealNonceRepo)
        {
            services.AddKeyedSingleton<INonceRepository>(
                optionsName,
                (sp, _) => factory!(sp));
        }
        else
        {
            services.AddKeyedSingleton<INonceRepository>(
                optionsName,
                new DefaultNonceRepository());
        }

        services.AddSingleton<IValidateOptions<BewitOptions>>(
            new BewitNonceRequirementValidator(optionsName, hasRealNonceRepo));

        services.AddSingleton<IBewitTokenRevoker<T>>(sp =>
        {
            var repo = sp.GetRequiredKeyedService<INonceRepository>(optionsName);

            return new BewitTokenRevoker<T>(repo);
        });
    }
}
