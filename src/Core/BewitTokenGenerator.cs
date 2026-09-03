namespace Bewit;

internal sealed class BewitTokenGenerator<TPayload>(
    BewitTokenRegistration<TPayload> registration,
    IOptionsMonitor<BewitTokenConfiguration> configurations,
    IEnumerable<IBewitTokenCodec<TPayload>> codecs,
    IBewitTokenRepository<TPayload> repository,
    IBewitTokenIdGenerator tokenIdGenerator,
    TimeProvider timeProvider)
    : IBewitTokenGenerator<TPayload>
    where TPayload : notnull
{
    public async ValueTask<BewitToken<TPayload>> GenerateAsync(
        TPayload payload,
        BewitTokenOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        BewitTokenConfiguration configuration = configurations.Get(registration.Purpose);
        DateTimeOffset createdAt = timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = createdAt.Add(options?.Lifetime ?? configuration.Lifetime);
        if (expiresAt <= createdAt)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Token lifetime must be positive.");
        }

        var reference = new BewitTokenReference(
            BewitTokenFormat.V9, registration.Purpose, tokenIdGenerator.CreateTokenId());
        if (configuration.ExpirationMode == BewitExpirationMode.ServerControlled)
        {
            await repository.CreateAsync(
                new BewitTokenRecord(
                    reference,
                    expiresAt,
                    configuration.Usage,
                    BewitTokenStatus.Active,
                    options?.Identifier,
                    createdAt,
                    Metadata: options?.Metadata),
                cancellationToken);
        }

        V9BewitTokenCodec<TPayload> codec = codecs.OfType<V9BewitTokenCodec<TPayload>>().Single();
        string token = codec.Encode(
            reference,
            payload,
            configuration.ExpirationMode,
            configuration.ExpirationMode == BewitExpirationMode.SelfContained ? expiresAt : null);
        return new BewitToken<TPayload>(token);
    }
}
