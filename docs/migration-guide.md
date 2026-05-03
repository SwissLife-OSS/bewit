# Bewit Migration Guide (v1.x → v2.0)

## Breaking Changes Summary

| Change | Old (v1.x) | New (v2.0) |
|--------|-----------|------------|
| Target Framework | net8.0 | net10.0 |
| Serialization | Newtonsoft.Json | System.Text.Json |
| Nullable | disabled | enabled |
| Configuration | Manual `BewitOptions` / `IConfiguration` | `IOptions<T>` pattern with validation |
| DI Registration | 11+ overloads across projects | Single `services.AddBewit(Action<BewitBuilder>)` |
| Crypto signature | `GetHash<T>(string, DateTime, T)` | `GetHash<T>(Guid, DateTime?, T)` |
| Token class | mutable | immutable class with factory method |
| Nonce repository | `InsertOneAsync` / `TakeOneAsync` | + `ExtendExpiryAsync`, `UpdateExpiryAsync`, `DeleteIdentifierAsync` |
| HotChocolate | 15.0.0 | 15.1.11 |
| MongoDB Driver | 2.x | 3.0+ |

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
            o.SlidingWindow = TimeSpan.FromMinutes(30);
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

### Before (v1.x)
```json
{
  "Bewit": {
    "Secret": "...",
    "TokenDuration": "00:05:00"
  }
}
```

### After (v2.0)
Configuration is done via code with `IOptions<BewitOptions>`. Each payload type can have its own options:
```csharp
bewit.AddPayload<string>(p => p.ConfigureOptions(o =>
{
    o.Secret = "secret-for-strings";
    o.TokenDuration = TimeSpan.FromMinutes(1);
    o.ExpiryMode = ExpiryMode.SelfContained;
}));
```

## MongoDB Migration

### Before (v1.x)
```csharp
builder.AddPayload<T>().UseMongoPersistence(configuration, options => options.NonceUsage = NonceUsage.ReUse);
builder.AddPayload<T2>().UseMongoPersistence(configuration, options => options.NonceUsage = NonceUsage.ReUse);
```

### After (v2.0)

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
Expiry lives in the database, not in the token. Admins can extend or revoke tokens:
```csharp
bewit.ConfigureOptions(o => o.ExpiryMode = ExpiryMode.ServerControlled);
// or per-payload:
p.ConfigureOptions(o => o.ExpiryMode = ExpiryMode.ServerControlled);
```

### Sliding Window
Token expiry is extended on each successful validation:
```csharp
bewit.ConfigureOptions(o => o.SlidingWindow = TimeSpan.FromMinutes(30));
// or per-payload:
p.ConfigureOptions(o => o.SlidingWindow = TimeSpan.FromMinutes(30));
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
| `PayloadBuilder.UseSlidingWindow(TimeSpan)` | `ConfigureOptions(o => o.SlidingWindow = ...)` |
| `PayloadBuilder.WithTokenDuration(TimeSpan)` | `ConfigureOptions(o => o.TokenDuration = ...)` |
| `UseMongoPersistence(config, ...)` | `UseMongoDb(...)` on `BewitBuilder` or `PayloadBuilder<T>` |
