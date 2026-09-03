namespace Bewit;

public sealed class BewitBuilder
{
    private readonly HashSet<Type> _payloadTypes = [];
    private readonly HashSet<string> _purposes = new(StringComparer.Ordinal);

    internal BewitBuilder(IServiceCollection services) => Services = services;

    public IServiceCollection Services { get; }
    internal string? ConfigurationSection { get; private set; }
    internal Action<BewitOptions>? ConfigureAction { get; private set; }
    internal IList<Action<IServiceCollection>> Registrations { get; } = [];

    public BewitBuilder BindConfiguration(string sectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ConfigurationSection = sectionName;
        return this;
    }

    public BewitBuilder Configure(Action<BewitOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ConfigureAction += configure;
        return this;
    }

    public BewitBuilder UseSigningKey(string keyId, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return Configure(options =>
        {
            options.CurrentKeyId = keyId;
            options.SigningKeys[keyId] = key;
        });
    }

    public BewitBuilder AddToken<TPayload>(
        string purpose,
        Action<BewitTokenBuilder<TPayload>>? configure = null)
        where TPayload : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);

        if (!_payloadTypes.Add(typeof(TPayload)))
        {
            throw new InvalidOperationException(
                $"Payload type '{typeof(TPayload)}' is already registered.");
        }

        if (!_purposes.Add(purpose))
        {
            throw new InvalidOperationException($"Token purpose '{purpose}' is already registered.");
        }

        var tokenBuilder = new BewitTokenBuilder<TPayload>(Services, purpose);
        configure?.Invoke(tokenBuilder);
        Registrations.Add(tokenBuilder.Register);
        return this;
    }

    public BewitBuilder AddStateStore<TStore>()
        where TStore : class, IBewitTokenStateStore
    {
        Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IBewitTokenStateStore, TStore>());
        return this;
    }
}
