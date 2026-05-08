using Microsoft.Extensions.DependencyInjection;

namespace Bewit;

public sealed class PayloadBuilder<T> where T : notnull
{
    internal string? ConfigurationSection { get; private set; }

    internal Action<BewitOptions>? OptionsOverride { get; private set; }

    internal Func<IServiceProvider, INonceRepository>? NonceRepositoryFactory { get; private set; }

    public PayloadBuilder<T> BindConfiguration(string sectionPath)
    {
        ConfigurationSection = sectionPath;

        return this;
    }

    public PayloadBuilder<T> ConfigureOptions(Action<BewitOptions> configure)
    {
        OptionsOverride = configure;

        return this;
    }

    public PayloadBuilder<T> UseNonceRepository(
        Func<IServiceProvider, INonceRepository> factory)
    {
        NonceRepositoryFactory = factory;

        return this;
    }
}
