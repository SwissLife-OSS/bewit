using Bewit.Exceptions;
using Microsoft.Extensions.Options;

namespace Bewit.Validation;

internal sealed class BewitTokenValidator<T>(
    IOptions<BewitOptions> options,
    ICryptographyService cryptographyService,
    INonceRepository nonceRepository,
    IVariablesProvider variablesProvider)
    : IBewitTokenValidator<T>
    where T : notnull
{
    private readonly BewitOptions _options = options.Value;

    public async ValueTask<T> ValidateBewitTokenAsync(
        BewitToken<T> token,
        CancellationToken cancellationToken)
    {
        var bewit = BewitSerializer.Deserialize<T>((string)token)
            ?? throw new BewitInvalidException();

        bool isSelfContained = _options.ExpiryMode == ExpiryMode.SelfContained;

        // Re-compute hash and verify integrity
        DateTime? hashExpiry = isSelfContained ? bewit.Token.ExpirationDate : null;
        string expectedHash = cryptographyService.GetHash(
            bewit.Token.Nonce, hashExpiry, bewit.Payload);

        if (!string.Equals(bewit.Hash, expectedHash, StringComparison.Ordinal))
        {
            throw new BewitInvalidException();
        }

        if (isSelfContained)
        {
            ValidateSelfContainedExpiry(bewit);
        }
        else
        {
            await ValidateServerControlledAsync(bewit, cancellationToken);
        }

        return bewit.Payload;
    }

    private void ValidateSelfContainedExpiry(Bewit<T> bewit)
    {
        if (bewit.Token.ExpirationDate.HasValue
            && bewit.Token.ExpirationDate.Value < variablesProvider.UtcNow)
        {
            throw new BewitExpiredException();
        }
    }

    private async ValueTask ValidateServerControlledAsync(
        Bewit<T> bewit,
        CancellationToken cancellationToken)
    {
        Token? nonceRecord = await nonceRepository.TakeOneAsync(
            bewit.Token.Nonce, cancellationToken);

        if (nonceRecord is null || nonceRecord == Token.Empty)
        {
            throw new BewitNotFoundException();
        }

        if (nonceRecord.IsDeleted)
        {
            throw new BewitNotFoundException();
        }

        if (nonceRecord.ExpirationDate.HasValue
            && nonceRecord.ExpirationDate.Value < variablesProvider.UtcNow)
        {
            throw new BewitExpiredException();
        }

        // Sliding window: extend the expiry on each successful validation
        if (_options.SlidingWindow.HasValue)
        {
            await nonceRepository.ExtendExpiryAsync(
                bewit.Token.Nonce,
                _options.SlidingWindow.Value,
                cancellationToken);
        }
    }
}
