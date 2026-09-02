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
    IEnumerable<IBewitTokenValidationEvents<T>> validationEvents)
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

        DateTime validatedAt = variablesProvider.UtcNow;

        await NotifyTokenValidatingAsync(
            bewit,
            validatedAt,
            isSelfContained ? bewit.Token.ExpirationDate : null,
            isSelfContained ? ExpiryMode.SelfContained : ExpiryMode.ServerControlled,
            cancellationToken);

        if (isSelfContained)
        {
            await ValidateSelfContainedExpiryAsync(
                bewit, validatedAt, cancellationToken);
        }
        else
        {
            await ValidateServerControlledAsync(
                bewit, validatedAt, cancellationToken);
        }

        return bewit.Payload;
    }

    private async ValueTask ValidateSelfContainedExpiryAsync(
        Bewit<T> bewit,
        DateTime validatedAt,
        CancellationToken cancellationToken)
    {
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
        DateTime validatedAt,
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

    private async ValueTask NotifyTokenValidatingAsync(
        Bewit<T> bewit,
        DateTime validatedAt,
        DateTime? tokenExpirationDate,
        ExpiryMode expiryMode,
        CancellationToken cancellationToken)
    {
        var context = new BewitTokenValidatingContext<T>(
            bewit.Payload,
            bewit.Token.Nonce,
            validatedAt,
            expiryMode,
            tokenExpirationDate);

        foreach (IBewitTokenValidationEvents<T> events in validationEvents)
        {
            await events.OnValidatingAsync(context, cancellationToken);
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

        foreach (IBewitTokenValidationEvents<T> events in validationEvents)
        {
            await events.OnExpiredAsync(context, cancellationToken);
        }
    }
}
