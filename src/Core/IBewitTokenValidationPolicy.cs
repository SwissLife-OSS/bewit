namespace Bewit;

public interface IBewitTokenValidationPolicy<TPayload> where TPayload : notnull
{
    ValueTask<BewitTokenValidationPolicyResult> OnValidatingAsync(
        BewitTokenValidationContext<TPayload> context,
        CancellationToken cancellationToken);
}
