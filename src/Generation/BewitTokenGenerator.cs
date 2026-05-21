using Microsoft.Extensions.Options;

namespace Bewit.Generation;

internal sealed class BewitTokenGenerator<T>(
    IOptions<BewitOptions> options,
    ICryptographyService cryptographyService,
    INonceRepository nonceRepository,
    IVariablesProvider variablesProvider)
    : IBewitTokenGenerator<T>
    where T : notnull
{
    private readonly BewitOptions _options = options.Value;

    public async ValueTask<BewitToken<T>> GenerateBewitTokenAsync(
        T payload,
        BewitTokenOptions? tokenOptions,
        CancellationToken cancellationToken)
    {
        TimeSpan duration = tokenOptions?.Duration ?? _options.TokenDuration;
        DateTime expirationDate = variablesProvider.UtcNow.Add(duration);
        Guid nonce = variablesProvider.NextToken;

        bool isSelfContained = _options.ExpiryMode == ExpiryMode.SelfContained;

        // SelfContained: expiry is embedded in the hash (tamper-proof)
        // ServerControlled: expiry is NOT in the hash (admin can extend/revoke via DB)
        DateTime? hashExpiry = isSelfContained ? expirationDate : null;

        string hash = cryptographyService.GetHash(nonce, hashExpiry, payload);

        if (!isSelfContained)
        {
            var dbToken = Token.Create(
                nonce,
                expirationDate,
                tokenOptions?.Identifier,
                tokenOptions?.ExtraProperties);

            await nonceRepository.InsertOneAsync(dbToken, cancellationToken);
        }

        // SelfContained tokens carry the expiry; ServerControlled tokens do not
        var bewitToken = Token.Create(
            nonce,
            isSelfContained ? expirationDate : null,
            null,
            null);

        var bewit = new Bewit<T>(bewitToken, payload, hash);

        return new BewitToken<T>(BewitSerializer.Serialize(bewit));
    }
}
