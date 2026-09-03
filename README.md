# Bewit

Bewit creates short-lived, purpose-bound HMAC tokens for links and API operations. Version 9 has one core package for issuing and validating tokens, a stable `bwt1` wire format, explicit key rotation, and a generic server-side token repository.

## Packages

| Package | Purpose |
| --- | --- |
| `Bewit` | Core contracts, v9 generation and validation |
| `Bewit.MongoDB` | Server-controlled v9 token state in MongoDB |
| `Bewit.AspNetCore` | ASP.NET Core token extraction and endpoint authorization |
| `Bewit.HotChocolate` | Hot Chocolate field middleware |
| `Bewit.Compatibility.V8` | Optional read-only v8 token validation |
| `Bewit.Compatibility.V8.MongoDB` | Optional access to the old `bewit_nonces` collection |

The v8 `Bewit.Generation`, `Bewit.Validation`, `Bewit.Storage.MongoDB`, `Bewit.Http`, and `Bewit.Extensions.Mvc` packages are discontinued.

## Registration

```csharp
services.AddBewit(bewit =>
{
    bewit.UseSigningKey("2026-09", configuration["Bewit:SigningKey"]!);

    bewit.UseMongoDb(mongo =>
    {
        mongo.ConnectionString = configuration.GetConnectionString("MongoDb")!;
        mongo.DatabaseName = "sharebox";
    });

    bewit.AddToken<UserShareLinkPayload>("share-link", token => token
        .UseServerControlledExpiration()
        .UseReusableTokens()
        .Configure(options => options.Lifetime = TimeSpan.FromDays(7)));
});
```

`AddToken<TPayload>` registers the generator, validator, and generic repository together. A payload type and a purpose can each be registered only once in a service provider. The purpose is a stable protocol value; do not derive it from a CLR type name.

The signing key must contain at least 32 UTF-8 bytes. To rotate keys, add the previous and new keys to `SigningKeys` and set `CurrentKeyId` to the new key. Existing tokens select their verification key by signed key ID.

## Generate and validate

```csharp
BewitToken<UserShareLinkPayload> token = await generator.GenerateAsync(
    payload,
    new BewitTokenOptions
    {
        Identifier = shareLink.Id.ToString(),
        Lifetime = TimeSpan.FromDays(7)
    },
    cancellationToken);

UserShareLinkPayload payload = await validator.ValidateAsync(token, cancellationToken);
```

New tokens always use `bwt1.<envelope>.<signature>`. The envelope and signature are Base64Url without padding. The signed envelope contains the version, key ID, purpose, expiration mode, token ID, optional embedded expiration, and payload. Validators sign the exact envelope bytes and never fall back to v8 after seeing the `bwt1` prefix.

## Server-controlled expiration and revocation

Server-controlled tokens are stored in the `bewit_tokens` collection by default. State is scoped by purpose and format.

```csharp
IBewitTokenRepository<UserShareLinkPayload> repository = ...;

BewitBulkOperationResult updated = await repository.UpdateExpirationByIdentifierAsync(
    shareLink.Id.ToString(),
    newExpiration,
    cancellationToken);

BewitBulkOperationResult revoked = await repository.RevokeByIdentifierAsync(
    shareLink.Id.ToString(),
    cancellationToken);
```

Expiration updates intentionally match active tokens even when their previous expiration has passed. This makes an expired share usable again after the application extends both its domain record and Bewit state. Bulk results report affected records per token format during migration.

`UpdateExpirationAsync(reference, ...)` updates one known token. `TryConsumeAsync` is atomic for single-use tokens. Self-contained tokens do not create repository state and cannot be single-use.

## Validation policies and observers

Policies run after signature verification and after server state has been loaded, but before status and expiration enforcement. They can perform an application-specific repair and request exactly one state reload.

```csharp
public sealed class LegacySharePolicy(
    IBewitTokenRepository<UserShareLinkPayload> repository,
    TimeProvider timeProvider)
    : IBewitTokenValidationPolicy<UserShareLinkPayload>
{
    public async ValueTask<BewitTokenValidationPolicyResult> OnValidatingAsync(
        BewitTokenValidationContext<UserShareLinkPayload> context,
        CancellationToken cancellationToken)
    {
        if (context.Reference.Format != BewitTokenFormat.V8
            || context.Record?.Identifier is not null)
        {
            return BewitTokenValidationPolicyResult.Continue;
        }

        await repository.UpdateExpirationAsync(
            context.Reference,
            timeProvider.GetUtcNow().AddDays(7),
            cancellationToken);

        return BewitTokenValidationPolicyResult.RefreshState;
    }
}
```

Register it on the token: `.AddValidationPolicy<LegacySharePolicy>()`. Policy exceptions fail validation. Observers use `IBewitTokenValidationObserver<TPayload>` for validated, expired, and rejected telemetry; observer failures are logged and never alter authentication.

## ASP.NET Core

```csharp
services.AddBewitAspNetCore(options =>
{
    options.HeaderName = "bewitToken";
    options.QueryParameterName = "bewit";
});

app.UseBewitEndpointAuthorization<UserShareLinkPayload>();
// or: endpoint.AddBewitAuthorization<UserShareLinkPayload>();
```

Missing tokens return 401 and invalid, expired, consumed, or revoked tokens return 403. A successful validation stores the typed payload on `HttpContext`; read it with `httpContext.GetBewitPayload<TPayload>()`.

## V8 to v9 migration

Install both compatibility packages only in applications that must accept links issued by v8:

```csharp
services.AddBewit(bewit =>
{
    bewit.UseSigningKey("v9-current", v9SigningKey);
    bewit.UseMongoDb(mongo =>
    {
        mongo.ConnectionString = mongoConnectionString;
        mongo.DatabaseName = databaseName;
        mongo.CollectionName = "bewit_tokens";
    });
    bewit.UseV8MongoDb("share-link", legacy =>
    {
        legacy.CollectionName = "bewit_nonces";
        legacy.Usage = BewitTokenUsage.Reusable;
    });

    bewit.AddToken<UserShareLinkPayload>("share-link", token => token
        .UseServerControlledExpiration()
        .AcceptV8Tokens(v8 =>
        {
            v8.Secret = legacySecret;
            v8.ExpirationMode = BewitExpirationMode.ServerControlled;
            v8.PayloadTypeName = "ShareBox.Api.UserShareLinkPayload";
            v8.AcceptUntil = DateTimeOffset.Parse("2026-12-01T00:00:00Z");
        }));
});
```

There is no legacy issuing switch: after deploying v9, every newly generated token is v9. Unprefixed input is dispatched only to the v8 codec. `bwt1` input is dispatched only to v9.

Use a blue-green or otherwise atomic traffic cutover. A mixed deployment is unsafe because an old v8 instance cannot validate a v9 token created by a new instance. Keep `bewit_nonces` untouched while compatibility is enabled; v9 writes only to `bewit_tokens`. Remove both compatibility packages after the longest v8 lifetime and operational grace period have elapsed.

See [the v9 migration guide](docs/migration-guide-v9.md) for the rollout and rollback checklist.
