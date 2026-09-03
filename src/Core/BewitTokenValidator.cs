using Bewit.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bewit;

internal sealed class BewitTokenValidator<TPayload>(
    BewitTokenRegistration<TPayload> registration,
    IEnumerable<IBewitTokenCodec<TPayload>> codecs,
    IBewitTokenRepository<TPayload> repository,
    IEnumerable<IBewitTokenValidationPolicy<TPayload>> policies,
    IEnumerable<IBewitTokenValidationObserver<TPayload>> observers,
    TimeProvider timeProvider,
    ILogger<BewitTokenValidator<TPayload>> logger)
    : IBewitTokenValidator<TPayload>
    where TPayload : notnull
{
    public async ValueTask<TPayload> ValidateAsync(
        BewitToken<TPayload> token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        string value = token.ToString();
        IBewitTokenCodec<TPayload>? codec = ResolveCodec(value);
        if (codec is null)
        {
            await NotifyRejectedAsync(null, BewitTokenRejectionReason.Invalid, null, cancellationToken);
            throw new BewitInvalidException();
        }

        BewitDecodedToken<TPayload> decoded;
        try
        {
            decoded = codec.DecodeAndVerify(value);
        }
        catch (Exception exception) when (exception is BewitException)
        {
            await NotifyRejectedAsync(codec.Format, BewitTokenRejectionReason.Invalid, exception, cancellationToken);
            throw;
        }

        if (!string.Equals(decoded.Reference.Purpose, registration.Purpose, StringComparison.Ordinal))
        {
            await NotifyRejectedAsync(codec.Format, BewitTokenRejectionReason.Invalid, null, cancellationToken);
            throw new BewitInvalidException();
        }

        BewitTokenRecord? record = decoded.ExpirationMode == BewitExpirationMode.ServerControlled
            ? await repository.GetAsync(decoded.Reference, cancellationToken)
            : null;
        DateTimeOffset? expiration = decoded.EmbeddedExpiration ?? record?.ExpiresAt;
        var context = new BewitTokenValidationContext<TPayload>(
            decoded.Payload, decoded.Reference, decoded.ExpirationMode, expiration, record);

        bool refresh = false;
        try
        {
            foreach (IBewitTokenValidationPolicy<TPayload> policy in policies)
            {
                refresh |= await policy.OnValidatingAsync(context, cancellationToken)
                    == BewitTokenValidationPolicyResult.RefreshState;
            }
        }
        catch (Exception exception)
        {
            await NotifyRejectedAsync(codec.Format, BewitTokenRejectionReason.PolicyFailed, exception, cancellationToken);
            throw new BewitPolicyException(exception);
        }

        if (refresh && decoded.ExpirationMode == BewitExpirationMode.ServerControlled)
        {
            record = await repository.GetAsync(decoded.Reference, cancellationToken);
            expiration = record?.ExpiresAt;
            context = context with { Record = record, Expiration = expiration };
        }

        if (decoded.ExpirationMode == BewitExpirationMode.ServerControlled)
        {
            await ValidateStoredTokenAsync(context, cancellationToken);
        }

        if (expiration is null || expiration <= timeProvider.GetUtcNow())
        {
            await NotifyExpiredAsync(context, cancellationToken);
            throw new BewitExpiredException();
        }

        if (record?.Usage == BewitTokenUsage.SingleUse)
        {
            BewitTokenConsumeResult consumed = await repository.TryConsumeAsync(
                decoded.Reference, timeProvider.GetUtcNow(), cancellationToken);
            await ValidateConsumeResultAsync(consumed, context, cancellationToken);
        }

        await NotifyValidatedAsync(context, cancellationToken);
        return decoded.Payload;
    }

    private IBewitTokenCodec<TPayload>? ResolveCodec(string value)
    {
        if (value.StartsWith(V9BewitTokenCodec<TPayload>.Prefix, StringComparison.Ordinal))
        {
            return codecs.SingleOrDefault(codec => codec.Format == BewitTokenFormat.V9 && codec.CanRead(value));
        }
        return codecs.SingleOrDefault(codec => codec.Format == BewitTokenFormat.V8 && codec.CanRead(value));
    }

    private async ValueTask ValidateStoredTokenAsync(
        BewitTokenValidationContext<TPayload> context,
        CancellationToken cancellationToken)
    {
        if (context.Record is null)
        {
            await NotifyRejectedAsync(context.Reference.Format, BewitTokenRejectionReason.NotFound, null, cancellationToken);
            throw new BewitNotFoundException();
        }
        if (context.Record.Status == BewitTokenStatus.Revoked)
        {
            await NotifyRejectedAsync(context.Reference.Format, BewitTokenRejectionReason.Revoked, null, cancellationToken);
            throw new BewitRevokedException();
        }
        if (context.Record.Status == BewitTokenStatus.Consumed)
        {
            await NotifyRejectedAsync(context.Reference.Format, BewitTokenRejectionReason.AlreadyConsumed, null, cancellationToken);
            throw new BewitAlreadyConsumedException();
        }
    }

    private async ValueTask ValidateConsumeResultAsync(
        BewitTokenConsumeResult result,
        BewitTokenValidationContext<TPayload> context,
        CancellationToken cancellationToken)
    {
        switch (result.Status)
        {
            case BewitTokenConsumeStatus.Consumed:
                return;
            case BewitTokenConsumeStatus.Expired:
                await NotifyExpiredAsync(context, cancellationToken);
                throw new BewitExpiredException();
            case BewitTokenConsumeStatus.Revoked:
                await NotifyRejectedAsync(context.Reference.Format, BewitTokenRejectionReason.Revoked, null, cancellationToken);
                throw new BewitRevokedException();
            case BewitTokenConsumeStatus.AlreadyConsumed:
                await NotifyRejectedAsync(context.Reference.Format, BewitTokenRejectionReason.AlreadyConsumed, null, cancellationToken);
                throw new BewitAlreadyConsumedException();
            default:
                await NotifyRejectedAsync(context.Reference.Format, BewitTokenRejectionReason.NotFound, null, cancellationToken);
                throw new BewitNotFoundException();
        }
    }

    private async ValueTask NotifyValidatedAsync(BewitTokenValidationContext<TPayload> context, CancellationToken ct)
    {
        foreach (IBewitTokenValidationObserver<TPayload> observer in observers)
            await ObserveAsync(() => observer.OnValidatedAsync(context, ct));
    }

    private async ValueTask NotifyExpiredAsync(BewitTokenValidationContext<TPayload> context, CancellationToken ct)
    {
        foreach (IBewitTokenValidationObserver<TPayload> observer in observers)
            await ObserveAsync(() => observer.OnExpiredAsync(context, ct));
    }

    private async ValueTask NotifyRejectedAsync(BewitTokenFormat? format, BewitTokenRejectionReason reason, Exception? exception, CancellationToken ct)
    {
        var context = new BewitTokenRejectedContext(registration.Purpose, format, reason, exception);
        foreach (IBewitTokenValidationObserver<TPayload> observer in observers)
            await ObserveAsync(() => observer.OnRejectedAsync(context, ct));
    }

    private async ValueTask ObserveAsync(Func<ValueTask> callback)
    {
        try
        { await callback(); }
        catch (Exception exception) { logger.LogWarning(exception, "A Bewit validation observer failed."); }
    }
}
