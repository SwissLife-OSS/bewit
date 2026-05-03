# Bewit

**Bewit is an authentication scheme for secure, temporary access tokens.**

Bewit enables authentication in use cases where cookies and auth headers can't be used — file downloads, temporary links, single-use tokens, and share links with admin-controlled expiry.

## Features

- **Self-Contained tokens** — expiry embedded in the token (stateless)
- **Server-Controlled tokens** — expiry managed in the database (admin can extend/revoke)
- **Sliding window** — token expiry extends on each successful validation
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

## Server-Controlled Tokens with Sliding Window

```csharp
services.AddBewit(bewit =>
{
    bewit.ConfigureOptions(o =>
    {
        o.Secret = "your-secret";
        o.TokenDuration = TimeSpan.FromDays(7);
        o.ExpiryMode = ExpiryMode.ServerControlled;
        o.SlidingWindow = TimeSpan.FromHours(1);
    });

    bewit.UseMongoDb(mongo =>
    {
        mongo.ConnectionString = "mongodb://localhost:27017";
        mongo.DatabaseName = "myapp";
    });

    bewit.AddPayload<ShareLinkPayload>();
});
```

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

Or reuse an existing `IMongoDatabase` from DI:

```csharp
bewit.UseMongoDb(sp => sp.GetRequiredService<IMyDbContext>().Database);
```

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
app.UseBewitTokenHeaderExtraction();
```

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

## Migration from v1.x

See [Migration Guide](docs/migration-guide.md).

## Community

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [Swiss Life OSS Code of Conduct](https://swisslife-oss.github.io/coc).
