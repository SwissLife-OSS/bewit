using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bewit;

internal static class PayloadRegistrationHelper
{
    public static void Register<T>(
        IServiceCollection services,
        PayloadBuilder<T> builder,
        Action<BewitOptions>? globalOptions,
        Func<IServiceProvider, INonceRepository>? builderNonceRepositoryFactory)
        where T : notnull
    {
        string optionsName = typeof(T).FullName ?? typeof(T).Name;

        services.AddOptions<BewitOptions>(optionsName)
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

        if (factory is not null)
        {
            services.AddKeyedSingleton<INonceRepository>(
                optionsName,
                (sp, _) => factory(sp));
        }
        else
        {
            services.AddKeyedSingleton<INonceRepository>(
                optionsName,
                new DefaultNonceRepository());
        }
    }
}
