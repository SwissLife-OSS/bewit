namespace Bewit;

public sealed class BewitTokenBuilder<TPayload> where TPayload : notnull
{
    private readonly IList<Action<IServiceCollection>> _registrations = [];

    internal BewitTokenBuilder(IServiceCollection services, string purpose)
    {
        Services = services;
        Purpose = purpose;
    }

    public IServiceCollection Services { get; }
    public string Purpose { get; }
    internal string? ConfigurationSection { get; private set; }
    internal Action<BewitTokenConfiguration>? ConfigureAction { get; private set; }

    public BewitTokenBuilder<TPayload> BindConfiguration(string sectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ConfigurationSection = sectionName;
        return this;
    }

    public BewitTokenBuilder<TPayload> Configure(Action<BewitTokenConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ConfigureAction += configure;
        return this;
    }

    public BewitTokenBuilder<TPayload> UseSelfContainedExpiration() =>
        Configure(options => options.ExpirationMode = BewitExpirationMode.SelfContained);

    public BewitTokenBuilder<TPayload> UseServerControlledExpiration() =>
        Configure(options => options.ExpirationMode = BewitExpirationMode.ServerControlled);

    public BewitTokenBuilder<TPayload> UseSingleUseTokens() =>
        Configure(options => options.Usage = BewitTokenUsage.SingleUse);

    public BewitTokenBuilder<TPayload> UseReusableTokens() =>
        Configure(options => options.Usage = BewitTokenUsage.Reusable);

    public BewitTokenBuilder<TPayload> AddValidationPolicy<TPolicy>()
        where TPolicy : class, IBewitTokenValidationPolicy<TPayload>
    {
        _registrations.Add(services =>
            services.AddScoped<IBewitTokenValidationPolicy<TPayload>, TPolicy>());
        return this;
    }

    public BewitTokenBuilder<TPayload> AddValidationObserver<TObserver>()
        where TObserver : class, IBewitTokenValidationObserver<TPayload>
    {
        _registrations.Add(services =>
            services.AddScoped<IBewitTokenValidationObserver<TPayload>, TObserver>());
        return this;
    }

    public BewitTokenBuilder<TPayload> AddTokenCodec<TCodec>()
        where TCodec : class, IBewitTokenCodec<TPayload>
    {
        _registrations.Add(services =>
            services.AddSingleton<IBewitTokenCodec<TPayload>, TCodec>());
        return this;
    }

    internal void Register(IServiceCollection services)
    {
        services.AddSingleton(new BewitTokenRegistration<TPayload>(Purpose));
        services.AddSingleton<IValidateOptions<BewitTokenConfiguration>,
            BewitTokenStorageValidator<TPayload>>();

        OptionsBuilder<BewitTokenConfiguration> options = services.AddOptions<BewitTokenConfiguration>(Purpose);
        if (ConfigurationSection is not null)
        {
            options.BindConfiguration(ConfigurationSection);
        }

        if (ConfigureAction is not null)
        {
            options.Configure(ConfigureAction);
        }

        options.Services.AddSingleton<IValidateOptions<BewitTokenConfiguration>>(
            new BewitTokenConfigurationValidator(Purpose));
        options.ValidateOnStart();

        services.AddSingleton<IBewitTokenCodec<TPayload>, V9BewitTokenCodec<TPayload>>();
        services.AddScoped<IBewitTokenRepository<TPayload>, BewitTokenRepository<TPayload>>();
        services.AddScoped<IBewitTokenGenerator<TPayload>, BewitTokenGenerator<TPayload>>();
        services.AddScoped<IBewitTokenValidator<TPayload>, BewitTokenValidator<TPayload>>();

        foreach (Action<IServiceCollection> registration in _registrations)
        {
            registration(services);
        }
    }
}
