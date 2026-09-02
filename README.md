# Bewit

**Bewit is an authentication scheme for secure, temporary access tokens.**

Bewit enables authentication in use cases where cookies and auth headers can't be used — file downloads, temporary links, single-use tokens, and share links with admin-controlled expiry.

## Features

- **Self-Contained tokens** — expiry embedded in the token (stateless)
- **Server-Controlled tokens** — expiry managed in the database; requires `UseMongoDb()` or `UseNonceRepository()`
- **Token revocation** — type-safe `IBewitTokenRevoker<T>` to revoke tokens by identifier
- **Multi-tenancy** — different secrets and modes per payload type
- **MongoDB persistence** — with OIDC auth support (Azure.Identity)
- **HotChocolate integration** — `[Bewit<T>]` attribute for GraphQL resolvers
- **MVC integration** — `[BewitMvc]`, `[FromBewit]`, `[BewitUrlAuthorization]` filters
- **Aspire-ready** — connection string pattern for distributed apps

## Quick Start

### Install

```bash
dotnet add package Bewit
dotnet add package Bewit.Generation
dotnet add package Bewit.Validation
```

### Registration

**From appsettings.json:**
```json
{
  "Bewit": {
    "Secret": "your-secret-at-least-32-chars!",
    "TokenDuration": "00:05:00",
    "ExpiryMode": "SelfContained"
  }
}
```

```csharp
services.AddBewit(bewit =>
{
    bewit.BindConfiguration("Bewit");
    bewit.AddPayload<string>();
});

services.AddBewitGeneration<string>();
services.AddBewitValidation<string>();
```

**Or code-only:**
```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureOptions(o =>
    {
        o.Secret = "your-secret-at-least-32-chars!";
        o.TokenDuration = TimeSpan.FromMinutes(5);
        o.ExpiryMode = ExpiryMode.SelfContained;
    });

    bewit.AddPayload<string>();
});

services.AddBewitGeneration<string>();
services.AddBewitValidation<string>();
```

Both can be combined — `BindConfiguration` loads from appsettings first, then `ConfigureOptions` overrides specific values.

## Configuration

v7.0 supports `appsettings.json` binding, code-based configuration, or both. When combined, code wins (standard .NET options layering: Bind → Configure → PostConfigure).

### From appsettings.json
```json
{
  "Bewit": {
    "Secret": "your-secret-at-least-32-chars!",
    "TokenDuration": "00:05:00",
    "ExpiryMode": "SelfContained"
  }
}
```

```csharp
services.AddBewit(bewit =>
{
    bewit.BindConfiguration("Bewit");
    bewit.AddPayload<string>();
});
```

### Code overrides on top of config

Aspire-friendly — `IConfiguration` is read at resolve time, not registration time:
```csharp
services.AddBewit(bewit =>
{
    bewit.BindConfiguration("Bewit");
    bewit.ConfigureOptions(o =>
    {
        o.TokenDuration = TimeSpan.FromMinutes(30); // overrides appsettings value
    });
    bewit.AddPayload<string>();
});
```

### Per-payload config section

Each payload type can bind to its own config section:
```json
{
  "Bewit": {
    "Secret": "global-secret",
    "TokenDuration": "00:05:00"
  },
  "Bewit:Downloads": {
    "Secret": "download-secret",
    "TokenDuration": "00:01:00",
    "ExpiryMode": "ServerControlled"
  }
}
```

```csharp
services.AddBewit(bewit =>
{
    bewit.BindConfiguration("Bewit");
    bewit.AddPayload<string>();
    bewit.AddPayload<DownloadPayload>(p =>
        p.BindConfiguration("Bewit:Downloads")); // overrides global section
});
```

### Code-only (no appsettings)
```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureOptions(o =>
    {
        o.Secret = "your-secret";
        o.TokenDuration = TimeSpan.FromMinutes(5);
    });
    bewit.AddPayload<string>();
});
```

### Generate a Token

```csharp
var generator = serviceProvider.GetRequiredService<IBewitTokenGenerator<string>>();

BewitToken<string> token = await generator.GenerateBewitTokenAsync(
    "my-payload", null, cancellationToken);
```

### Validate a Token

```csharp
var validator = serviceProvider.GetRequiredService<IBewitTokenValidator<string>>();

string payload = await validator.ValidateBewitTokenAsync(token, cancellationToken);
```

### Validation Events

Register one or more singleton event handlers to run application logic after token
integrity has been verified. `OnValidatingAsync` runs before expiry and nonce validation;
`OnExpiredAsync` runs immediately before `BewitExpiredException` is thrown.

```csharp
public sealed class ShareLinkValidationEvents(
    INonceRepository nonceRepository,
    IShareLinkRepository shareLinks,
    IExpiredTokenTelemetry telemetry)
    : IBewitTokenValidationEvents<ShareLinkPayload>
{
    public async ValueTask OnValidatingAsync(
        BewitTokenValidatingContext<ShareLinkPayload> context,
        CancellationToken cancellationToken)
    {
        // Example application policy: synchronize a server-controlled nonce
        // before Bewit loads and validates it.
        ShareLink link = await shareLinks.GetAsync(
            context.Payload.ShareId, cancellationToken);

        if (context.ExpiryMode == ExpiryMode.ServerControlled
            && link.ExpiresAt > context.ValidatedAt)
        {
            await nonceRepository.UpdateExpiryAsync(
                context.Nonce, link.ExpiresAt, cancellationToken);
        }
    }

    public ValueTask OnExpiredAsync(
        BewitTokenExpiredContext<ShareLinkPayload> context,
        CancellationToken cancellationToken)
    {
        telemetry.RecordExpiredToken(
            context.Payload.ShareId,
            context.ExpirationDate,
            context.ValidatedAt);

        return ValueTask.CompletedTask;
    }
}

services.AddBewitTokenValidationEvents<ShareLinkPayload>(
    (sp, nonceRepository) => new ShareLinkValidationEvents(
        nonceRepository,
        sp.GetRequiredService<IShareLinkRepository>(),
        sp.GetRequiredService<IExpiredTokenTelemetry>()));
```

Both contexts provide the trusted payload, token nonce, validation time, and expiry
mode without exposing the raw token or hash. `TokenExpirationDate` on the validating
context contains the signed expiry for `SelfContained` tokens and is `null` for
`ServerControlled` tokens because the handler runs before the nonce repository is read.
`ExpirationDate` on the expired context is always authoritative: it comes from the
signed token for `SelfContained` and the nonce repository for `ServerControlled`.
Handlers run sequentially in registration order, must be thread-safe, and should not
throw exceptions.

## Server-Controlled Tokens

`ExpiryMode.ServerControlled` requires a persistent nonce repository.
The app fails at startup if `ServerControlled` is set without `UseMongoDb()` / `UseNonceRepository()`.

```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureOptions(o =>
    {
        o.Secret = "your-secret";
        o.TokenDuration = TimeSpan.FromDays(7);
        o.ExpiryMode = ExpiryMode.ServerControlled;
    });

    bewit.UseMongoDb(mongo =>
    {
        mongo.ConnectionString = "mongodb://localhost:27017";
        mongo.DatabaseName = "myapp";
    });

    bewit.AddPayload<ShareLinkPayload>();
});
```

## Token Revocation

Inject `IBewitTokenRevoker<T>` to revoke all tokens for an identifier — no keyed DI magic strings needed:

```csharp
public class Mutation(IBewitTokenRevoker<BarPayload> revoker)
{
    public async Task<string> InvalidateTokens(string identifier, CancellationToken ct)
    {
        await revoker.RevokeByIdentifierAsync(identifier, ct);

        return identifier;
    }
}
```

## Updating Server-Controlled Expiry

Set an identifier when generating server-controlled tokens when their expiry must
later follow the lifetime of an owning resource. The configured nonce repository can
then update every active token for that identifier in one operation:

```csharp
await generator.GenerateBewitTokenAsync(
    payload,
    new BewitTokenOptions { Identifier = shareId },
    cancellationToken);

bool found = await nonceRepository.UpdateExpiryByIdentifierAsync(
    shareId,
    newExpirationDate,
    cancellationToken);
```

The operation returns `true` when at least one active nonce matched. Deleted or
consumed nonces are not updated. Custom nonce repositories can implement this
optional capability; repositories that do not support it throw
`NotSupportedException`.

## MongoDB with OIDC (Azure Cosmos DB)

```csharp
bewit.UseMongoDb(mongo =>
{
    mongo.ConnectionString = "mongodb+srv://...";
    mongo.DatabaseName = "mydb";
    mongo.AuthType = MongoAuthType.Oidc;
    mongo.OidcScopes = ["https://cosmos-db-scope/.default"];
});
```

Or reuse an existing `IMongoDatabase` from DI — the recommended approach when the service already uses [`MongoDB.Extensions.Context`](https://github.com/SwissLife-OSS/mongo-extensions):

```csharp
// Register the context once
services.AddMongoDbContext<MyDbContext, IMyDbContext>(
    configurationSection: "MongoDb");

// Bewit reuses its IMongoDatabase — no second connection opened
services.AddBewit(bewit =>
{
    bewit.UseMongoDb(
        sp => sp.GetRequiredService<IMyDbContext>().Database,
        mongo => { mongo.NonceUsage = NonceUsage.OneTime; });

    bewit.AddPayload<MyPayload>(p =>
        p.ConfigureOptions(o => o.ExpiryMode = ExpiryMode.ServerControlled));
});
```

This shares the connection pool and inherits the convention packs, serializer registrations, and read/write concerns configured on the context.

## Multiple Payloads with Shared MongoDB

All payloads inherit the builder-level MongoDB and options. Per-payload overrides are possible:

```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureOptions(o =>
    {
        o.Secret = "your-secret";
        o.ExpiryMode = ExpiryMode.ServerControlled;
    });

    bewit.UseMongoDb(mongo =>
    {
        mongo.ConnectionString = "mongodb://localhost:27017";
        mongo.DatabaseName = "myapp";
        mongo.NonceUsage = NonceUsage.ReUse;
    });

    bewit.AddPayload<NominationBewitContext>();
    bewit.AddPayload<UserRegistrationBewitContext>();
    bewit.AddPayload<DocumentDownloadBewitContext>();

    // Override: self-contained, no MongoDB needed
    bewit.AddPayload<DownloadBewitContext>(p =>
    {
        p.ConfigureOptions(o =>
        {
            o.ExpiryMode = ExpiryMode.SelfContained;
            o.TokenDuration = TimeSpan.FromMinutes(5);
        });
    });
});
```

## HotChocolate Integration

```bash
dotnet add package Bewit.Extensions.HotChocolate
```

### `[Bewit<T>]` Attribute

```csharp
[Mutation]
[Bewit<NominationBewitContext>(ExceptionType = typeof(BewitValidationException))]
public static async Task<NominationDto> AssignNomineeAsync(
    [Service] IHttpContextAccessor httpContextAccessor, ...)
{
    var context = httpContextAccessor.GetBewitPayload<NominationBewitContext>();
}
```

### Setup

```csharp
app.UseBewitTokenExtraction();
```

## HTTP Endpoint Integration

```bash
dotnet add package Bewit.Http
```

### Minimal API — Endpoint Filter (recommended)

Apply authorization to individual routes or route groups:
```csharp
app.MapGet("/files/{id}", (string id) => ...)
    .AddBewitAuthorization<MyPayload>();

// or protect a group of endpoints:
app.MapGroup("/api/files")
    .AddBewitAuthorization<MyPayload>();
```

The filter validates the token from the configured header, query parameter, or pre-extracted `HttpContext.Items` entry, and makes the payload available via `GetBewitPayload<T>()`.

### Middleware (global)

Protect all endpoints via middleware:
```csharp
app.UseBewitEndpointAuthorization<MyPayload>();
```

## Token Extraction

All extensions (HotChocolate, Http, Mvc) read the bewit token from the same configurable sources: an HTTP **header** and/or a **query parameter**. Header takes precedence when both are present.

Defaults match the v6.x behavior:
- Header: `bewitToken`
- Query parameter: `bewit`

### Code configuration
```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureTokenExtraction(o =>
    {
        o.HeaderName = "X-Custom-Token";
        o.QueryParamName = "token";
    });

    bewit.AddPayload<string>();
});
```

### appsettings.json
```json
{
  "Bewit:TokenExtraction": {
    "HeaderName": "X-Custom-Token",
    "QueryParamName": "token"
  }
}
```

```csharp
services.AddBewit(bewit =>
{
    bewit.BindTokenExtractionConfiguration("Bewit:TokenExtraction");
    bewit.AddPayload<string>();
});
```

Both can be combined — `BindTokenExtractionConfiguration` loads from appsettings first, then `ConfigureTokenExtraction` overrides specific values (standard .NET options layering).

## MVC Integration

```bash
dotnet add package Bewit.Extensions.Mvc
```

```csharp
[BewitUrlAuthorization]
[HttpGet("download/{id}")]
public IActionResult Download(string id) { ... }
```

## Packages

| Package | Description |
|---------|-------------|
| `Bewit` | Core abstractions, models, crypto, DI |
| `Bewit.Generation` | Token generation |
| `Bewit.Validation` | Token validation |
| `Bewit.Storage.MongoDB` | MongoDB nonce repository with OIDC support |
| `Bewit.Extensions.HotChocolate` | HotChocolate `[Bewit<T>]`, middleware |
| `Bewit.Extensions.Mvc` | MVC filters and parameter binding |
| `Bewit.Http` | Minimal API endpoint authorization |

## Migration

See [Migration Guide v7](docs/migration-guide-v7.md).

## Community

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [Swiss Life OSS Code of Conduct](https://swisslife-oss.github.io/coc).
