# Bewit Migration Guide (v6.x → v7.0)

## Breaking Changes Summary

| Change | Old (v6.x) | New (v7.0) |
|--------|-----------|------------|
| Target Framework | net8.0 | net10.0 |
| Serialization | Newtonsoft.Json | System.Text.Json |
| Nullable | disabled | enabled |
| Configuration | Manual `BewitOptions` / `IConfiguration` | `BindConfiguration` + `ConfigureOptions` with `IOptions<T>` validation |
| DI Registration | 11+ overloads across projects | Single `services.AddBewit(Action<BewitBuilder>)` |
| Crypto signature | `GetHash<T>(string, DateTime, T)` | `GetHash<T>(Guid, DateTime?, T)` |
| Token class | mutable | immutable class with factory method |
| Nonce repository | `InsertOneAsync` / `TakeOneAsync` | + `UpdateExpiryAsync`, `DeleteIdentifierAsync` |
| HotChocolate | 15.0.0 | 15.1.11 |
| MongoDB Driver | 2.x | 3.0+ |
| HttpContextAccessor | Manual `services.AddHttpContextAccessor()` | Auto-registered by `AddBewit()` |
| Startup validation | None | `ServerControlled` without nonce repo fails at startup |
| Token extraction | Hardcoded header/query per extension | Unified `BewitTokenExtractionOptions` with header + query fallback |
| HotChocolate setup | `UseBewitTokenHeaderExtraction()` | `UseBewitTokenExtraction()` |

## DI Registration Migration

### Before (v6.x)
```csharp
services.AddBewitUrlAuthorizationFilter(configuration);
services.AddBewitGeneration(configuration, builder => { ... });
// Multiple overloads per scenario
```

### After (v7.0)
```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureOptions(o =>
    {
        o.Secret = "your-secret";
        o.TokenDuration = TimeSpan.FromMinutes(5);
        o.ExpiryMode = ExpiryMode.SelfContained;
    });

    bewit.AddPayload<string>();

    // Server-controlled with MongoDB — all via options
    bewit.AddPayload<MyCustomPayload>(p =>
    {
        p.ConfigureOptions(o =>
        {
            o.ExpiryMode = ExpiryMode.ServerControlled;
        });
        p.UseMongoDb(mongo =>
        {
            mongo.ConnectionString = "mongodb://...";
            mongo.DatabaseName = "mydb";
        });
    });
});

// Register generation/validation per payload type
services.AddBewitGeneration<string>();
services.AddBewitGeneration<MyCustomPayload>();
services.AddBewitValidation<string>();
services.AddBewitValidation<MyCustomPayload>();
```

## Configuration Migration

### Before (v6.x)
```json
{
  "Bewit": {
    "Secret": "...",
    "TokenDuration": "00:05:00"
  }
}
```

### After (v7.0)

`appsettings.json` stays the same:
```json
{
  "Bewit": {
    "Secret": "your-secret-at-least-32-chars!",
    "TokenDuration": "00:05:00",
    "ExpiryMode": "SelfContained"
  }
}
```

Bind it via the builder:
```csharp
services.AddBewit(bewit =>
{
    bewit.BindConfiguration("Bewit");
    bewit.AddPayload<string>();
});
```

For all configuration options (code overrides, per-payload sections, code-only) see the [README](../README.md#configuration).

## MongoDB Migration

### Before (v6.x)
```csharp
builder.AddPayload<T>().UseMongoPersistence(configuration, options => options.NonceUsage = NonceUsage.ReUse);
builder.AddPayload<T2>().UseMongoPersistence(configuration, options => options.NonceUsage = NonceUsage.ReUse);
```

### After (v7.0)

**Builder-level (shared across all payloads):**
```csharp
bewit.UseMongoDb(mongo =>
{
    mongo.ConnectionString = "mongodb://...";
    mongo.DatabaseName = "mydb";
    mongo.NonceUsage = NonceUsage.ReUse;
});

bewit.AddPayload<T>();   // inherits MongoDB above
bewit.AddPayload<T2>();  // inherits MongoDB above
```

**Per-payload override (when needed):**
```csharp
p.UseMongoDb(mongo =>
{
    mongo.ConnectionString = "mongodb://...";
    mongo.DatabaseName = "mydb";
    mongo.AuthType = MongoAuthType.Oidc;
    mongo.OidcScopes = ["https://cosmos-db-scope/.default"];
});
```

**Aspire connection string:**
```csharp
bewit.UseMongoDb("sharebox", mongo =>
{
    mongo.DatabaseName = "mydb";
});
```

**DI-based (reuse existing MongoDbContext):**
```csharp
bewit.UseMongoDb(sp => sp.GetRequiredService<IMyDbContext>().Database);
```

## New Features

### Server-Controlled Expiry
Expiry lives in the database, not in the token. Admins can extend or revoke tokens.
Requires a persistent nonce repository (`UseMongoDb()` or `UseNonceRepository()`) — the app will fail at startup if none is configured:
```csharp
bewit.ConfigureOptions(o => o.ExpiryMode = ExpiryMode.ServerControlled);
// or per-payload:
p.ConfigureOptions(o => o.ExpiryMode = ExpiryMode.ServerControlled);
```

### Token Revocation
Type-safe revocation via `IBewitTokenRevoker<T>`:
```csharp
IBewitTokenRevoker<BarPayload> revoker
await revoker.RevokeByIdentifierAsync(identifier, ct);
```

### `[Bewit<T>]` Attribute (HotChocolate)
Previously lived in consuming apps (e.g., onboard). Now part of the library:
```csharp
[Mutation]
[Bewit<NominationBewitContext>(ExceptionType = typeof(BewitValidationException))]
public static async Task<NominationDto> AssignNomineeAsync(
    [Service] IHttpContextAccessor httpContextAccessor, ...)
{
    var context = httpContextAccessor.GetBewitPayload<NominationBewitContext>();
}
```

### OIDC Authentication for MongoDB
Built-in OIDC support using `DefaultAzureCredential` — no dependency on mongo-extensions:
```csharp
p.UseMongoDb(mongo =>
{
    mongo.AuthType = MongoAuthType.Oidc;
    mongo.OidcScopes = ["https://cosmos-db-scope/.default"];
});
```

### Multi-Tenancy
Different payload types can have different secrets, expiry modes, and storage backends.
Builder-level `UseMongoDb()` is inherited by all payloads — per-payload overrides are possible:
```csharp
services.AddBewit(bewit =>
{
    // Shared MongoDB for all server-controlled payloads
    bewit.UseMongoDb(mongo =>
    {
        mongo.ConnectionString = "mongodb://...";
        mongo.DatabaseName = "mydb";
    });

    bewit.AddPayload<PublicLink>(p =>
    {
        p.ConfigureOptions(o =>
        {
            o.Secret = "public-secret";
            o.ExpiryMode = ExpiryMode.SelfContained;
        });
    });

    bewit.AddPayload<AdminLink>(p =>
    {
        p.ConfigureOptions(o =>
        {
            o.Secret = "admin-secret";
            o.ExpiryMode = ExpiryMode.ServerControlled;
        });
        // Inherits builder-level UseMongoDb
    });
});
```

## Removed Types

| Removed | Replacement |
|---------|-------------|
| `BewitConfiguration` | `BewitOptions` with `IValidateOptions` |
| `BewitContext` | Removed (HttpContext extensions instead) |
| `BewitPayloadContext` | `PayloadBuilder<T>` |
| `BewitRegistrationBuilder` | `BewitBuilder` |
| `CryptoAlgorithm` | Removed (HMAC-SHA256 only) |
| `IdentifiableToken` | `Token` with `Identifier` property |
| `InvalidSecretException` | `ArgumentException` in constructor |
| `Bewit.Validation.Exceptions.BewitException` | `Bewit.Exceptions.BewitException` (moved to Core) |
| `PayloadBuilder.UseServerControlled()` | `ConfigureOptions(o => o.ExpiryMode = ExpiryMode.ServerControlled)` |
| `PayloadBuilder.UseSelfContained()` | `ConfigureOptions(o => o.ExpiryMode = ExpiryMode.SelfContained)` |
| `PayloadBuilder.WithTokenDuration(TimeSpan)` | `ConfigureOptions(o => o.TokenDuration = ...)` |
| `UseMongoPersistence(config, ...)` | `UseMongoDb(...)` on `BewitBuilder` or `PayloadBuilder<T>` |
| `BewitTokenConstants` | `BewitTokenExtractionOptions` (configurable via options pattern) |
| `UseBewitTokenHeaderExtraction()` | `UseBewitTokenExtraction()` |

## Token Extraction Migration

In v6.x, the HotChocolate extension used a hardcoded `bewitToken` header and the Http extension used a hardcoded `bewit` query parameter. These were not configurable.

In v7.0, all extensions (HotChocolate, Http, Mvc) use `BewitTokenExtractionOptions` — a shared, configurable options class that follows the standard .NET options pattern.

### Before (v6.x)
```csharp
// HotChocolate — header only, hardcoded name
app.UseBewitTokenHeaderExtraction();

// Http — query param only, hardcoded name
app.UseBewitEndpointAuthorization<T>();

// Mvc — query param only, hardcoded name
[BewitUrlAuthorization]
```

### After (v7.0)
```csharp
// HotChocolate — reads header first, then query param
app.UseBewitTokenExtraction();

// Http — reads header first, then query param
app.UseBewitEndpointAuthorization<T>();

// Mvc — reads header first, then query param
[BewitUrlAuthorization]
```

All three now check **header first, then fall back to query parameter**. Default names are unchanged (`bewitToken` header, `bewit` query param), so existing consumers work without config changes.

### Custom token extraction
```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureTokenExtraction(o =>
    {
        o.HeaderName = "X-Custom-Token";
        o.QueryParamName = "token";
    });
    // or from appsettings.json:
    bewit.BindTokenExtractionConfiguration("Bewit:TokenExtraction");

    bewit.AddPayload<string>();
});
```

`BewitTokenExtractionOptions` supports full .NET options layering: `Bind` → `Configure` → `PostConfigure`, with `ValidateDataAnnotations` and `ValidateOnStart`.
