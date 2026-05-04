using Microsoft.Extensions.DependencyInjection;

namespace Bewit;

public sealed class BewitBuilder
{
    internal IServiceCollection Services { get; }

    internal string? ConfigurationSection { get; private set; }

    internal Action<BewitOptions>? GlobalOptionsAction { get; private set; }

    internal Func<IServiceProvider, INonceRepository>? NonceRepositoryFactory { get; private set; }

    internal Action<BewitTokenExtractionOptions>? TokenExtractionAction { get; private set; }

    internal List<Action<IServiceCollection>> PayloadRegistrations { get; } = [];

    internal BewitBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public BewitBuilder BindConfiguration(string sectionPath)
    {
        ConfigurationSection = sectionPath;

        return this;
    }

    public BewitBuilder ConfigureOptions(Action<BewitOptions> configure)
    {
        GlobalOptionsAction = configure;

        return this;
    }

    public BewitBuilder UseNonceRepository(
        Func<IServiceProvider, INonceRepository> factory)
    {
        NonceRepositoryFactory = factory;

        return this;
    }

    public BewitBuilder AddPayload<T>(Action<PayloadBuilder<T>>? configure = null) where T : notnull
    {
        var payloadBuilder = new PayloadBuilder<T>();
        configure?.Invoke(payloadBuilder);

        Func<IServiceProvider, INonceRepository>? builderFactory = NonceRepositoryFactory;
        string? section = ConfigurationSection;

        PayloadRegistrations.Add(services =>
            PayloadRegistrationHelper.Register(
                services, payloadBuilder, section, GlobalOptionsAction, builderFactory));

        return this;
    }

    public BewitBuilder ConfigureTokenExtraction(
        Action<BewitTokenExtractionOptions> configure)
    {
        TokenExtractionAction = configure;

        return this;
    }
}
