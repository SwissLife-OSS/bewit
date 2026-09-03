namespace Bewit;

internal sealed class BewitTokenStorageValidator<TPayload>(
    BewitTokenRegistration<TPayload> registration,
    IEnumerable<IBewitTokenStateStore> stores)
    : IValidateOptions<BewitTokenConfiguration>
    where TPayload : notnull
{
    public ValidateOptionsResult Validate(string? name, BewitTokenConfiguration options)
    {
        if (!string.Equals(name, registration.Purpose, StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Skip;
        }
        if (options.ExpirationMode == BewitExpirationMode.ServerControlled
            && !stores.Any(store => store.Format == BewitTokenFormat.V9
                && store.SupportsPurpose(registration.Purpose)))
        {
            return ValidateOptionsResult.Fail(
                $"Server-controlled token purpose '{registration.Purpose}' requires a v9 state store.");
        }
        return ValidateOptionsResult.Success;
    }
}
