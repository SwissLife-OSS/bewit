using System.Security.Cryptography;
using System.Text;
using Bewit.Exceptions;
using Microsoft.Extensions.Options;

namespace Bewit.Validation;

internal sealed class BewitTokenValidator<T>(
    IOptions<BewitOptions> options,
    ICryptographyService cryptographyService,
    INonceRepository nonceRepository,
    IVariablesProvider variablesProvider,
    IEnumerable<IBewitTokenValidationObserver<T>> observers)
    : IBewitTokenValidator<T>
    where T : notnull
{
    private readonly BewitOptions _options = options.Value;

    public async ValueTask<T> ValidateBewitTokenAsync(
        BewitToken<T> token,
        CancellationToken cancellationToken)
    {
        Bewit<T> bewit = BewitSerializer.Deserialize<T>((string)token)
            ?? throw new BewitInvalidException();

        bool isSelfContained = _options.ExpiryMode == ExpiryMode.SelfContained;

        // Re-compute hash and verify integrity
        DateTime? hashExpiry = isSelfContained ? bewit.Token.ExpirationDate : null;
        string expectedHash = cryptographyService.GetHash(
            bewit.Token.Nonce, hashExpiry, bewit.Payload);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(bewit.Hash),
                Encoding.UTF8.GetBytes(expectedHash)))
        {
            throw new BewitInvalidException();
        }

        if (isSelfContained)
        {
            await ValidateSelfContainedExpiryAsync(bewit, cancellationToken);
        }
        else
        {
            await ValidateServerControlledAsync(bewit, cancellationToken);
        }

        return bewit.Payload;
    }

    private async ValueTask ValidateSelfContainedExpiryAsync(
        Bewit<T> bewit,
        CancellationToken cancellationToken)
    {
        DateTime validatedAt = variablesProvider.UtcNow;

        if (bewit.Token.ExpirationDate is { } expirationDate
            && expirationDate < validatedAt)
        {
            await NotifyTokenExpiredAsync(
                bewit,
                expirationDate,
                validatedAt,
                ExpiryMode.SelfContained,
                cancellationToken);

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

        DateTime validatedAt = variablesProvider.UtcNow;

        if (nonceRecord.ExpirationDate is { } expirationDate
            && expirationDate < validatedAt)
        {
            await NotifyTokenExpiredAsync(
                bewit,
                expirationDate,
                validatedAt,
                ExpiryMode.ServerControlled,
                cancellationToken);

            throw new BewitExpiredException();
        }
    }

    private async ValueTask NotifyTokenExpiredAsync(
        Bewit<T> bewit,
        DateTime expirationDate,
        DateTime validatedAt,
        ExpiryMode expiryMode,
        CancellationToken cancellationToken)
    {
        var context = new BewitTokenExpiredContext<T>(
            bewit.Payload,
            bewit.Token.Nonce,
            expirationDate,
            validatedAt,
            expiryMode);

        foreach (IBewitTokenValidationObserver<T> observer in observers)
        {
            await observer.OnTokenExpiredAsync(context, cancellationToken);
        }
    }
}
