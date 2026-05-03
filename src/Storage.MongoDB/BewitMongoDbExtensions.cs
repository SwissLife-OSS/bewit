using Bewit.Storage.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Authentication.Oidc;

namespace Bewit;

public static class BewitMongoDbExtensions
{
    /// <summary>
    /// Configures MongoDB as the nonce repository for all payloads registered on this builder,
    /// unless a specific payload overrides it with its own <c>UseMongoDb</c> or <c>UseNonceRepository</c>.
    /// </summary>
    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        Action<BewitMongoOptions> configure)
    {
        builder.UseNonceRepository(sp =>
        {
            var mongoOptions = new BewitMongoOptions();
            configure(mongoOptions);

            IMongoDatabase database = CreateDatabase(mongoOptions);
            var options = Microsoft.Extensions.Options.Options.Create(mongoOptions);

            return new MongoNonceRepository(database, options);
        });

        return builder;
    }

    /// <summary>
    /// Configures MongoDB as the nonce repository for all payloads using an Aspire connection string name.
    /// </summary>
    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        string connectionStringName,
        Action<BewitMongoOptions>? configure = null)
    {
        builder.UseNonceRepository(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            string? connectionString = configuration.GetConnectionString(connectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' not found in configuration.");
            }

            var mongoOptions = new BewitMongoOptions { ConnectionString = connectionString };
            configure?.Invoke(mongoOptions);

            IMongoDatabase database = CreateDatabase(mongoOptions);
            var options = Microsoft.Extensions.Options.Options.Create(mongoOptions);

            return new MongoNonceRepository(database, options);
        });

        return builder;
    }

    /// <summary>
    /// Configures MongoDB as the nonce repository for all payloads using an existing <see cref="IMongoDatabase"/> from DI.
    /// </summary>
    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        Func<IServiceProvider, IMongoDatabase> databaseFactory,
        Action<BewitMongoOptions>? configure = null)
    {
        builder.UseNonceRepository(sp =>
        {
            IMongoDatabase database = databaseFactory(sp);

            var mongoOptions = new BewitMongoOptions
            {
                ConnectionString = "di-provided",
                DatabaseName = "di-provided"
            };

            configure?.Invoke(mongoOptions);

            var options = Microsoft.Extensions.Options.Options.Create(mongoOptions);

            return new MongoNonceRepository(database, options);
        });

        return builder;
    }

    /// <summary>
    /// Configures MongoDB as the nonce repository for this specific payload type.
    /// Overrides the builder-level <c>UseMongoDb</c> if set.
    /// </summary>
    public static PayloadBuilder<T> UseMongoDb<T>(
        this PayloadBuilder<T> builder,
        Action<BewitMongoOptions> configure)
        where T : notnull
    {
        builder.UseNonceRepository(sp =>
        {
            var mongoOptions = new BewitMongoOptions();
            configure(mongoOptions);

            IMongoDatabase database = CreateDatabase(mongoOptions);
            var options = Microsoft.Extensions.Options.Options.Create(mongoOptions);

            return new MongoNonceRepository(database, options);
        });

        return builder;
    }

    public static PayloadBuilder<T> UseMongoDb<T>(
        this PayloadBuilder<T> builder,
        string connectionStringName,
        Action<BewitMongoOptions>? configure = null)
        where T : notnull
    {
        builder.UseNonceRepository(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            string? connectionString = configuration.GetConnectionString(connectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connectionStringName}' not found in configuration.");
            }

            var mongoOptions = new BewitMongoOptions { ConnectionString = connectionString };
            configure?.Invoke(mongoOptions);

            IMongoDatabase database = CreateDatabase(mongoOptions);
            var options = Microsoft.Extensions.Options.Options.Create(mongoOptions);

            return new MongoNonceRepository(database, options);
        });

        return builder;
    }

    public static PayloadBuilder<T> UseMongoDb<T>(
        this PayloadBuilder<T> builder,
        Func<IServiceProvider, IMongoDatabase> databaseFactory,
        Action<BewitMongoOptions>? configure = null)
        where T : notnull
    {
        builder.UseNonceRepository(sp =>
        {
            IMongoDatabase database = databaseFactory(sp);

            var mongoOptions = new BewitMongoOptions
            {
                ConnectionString = "di-provided",
                DatabaseName = "di-provided"
            };

            configure?.Invoke(mongoOptions);

            var options = Microsoft.Extensions.Options.Options.Create(mongoOptions);

            return new MongoNonceRepository(database, options);
        });

        return builder;
    }

    private static IMongoDatabase CreateDatabase(BewitMongoOptions mongoOptions)
    {
        MongoClientSettings clientSettings = MongoClientSettings
            .FromConnectionString(mongoOptions.ConnectionString);

        if (mongoOptions.AuthType == MongoAuthType.Oidc)
        {
            if (mongoOptions.OidcScopes is null || mongoOptions.OidcScopes.Count == 0)
            {
                throw new InvalidOperationException(
                    "OidcScopes must be configured when AuthType is Oidc.");
            }

            clientSettings.Credential = MongoCredential
                .CreateOidcCredential(new BewitOidcCallback(mongoOptions.OidcScopes));
        }

        var client = new MongoClient(clientSettings);

        return client.GetDatabase(mongoOptions.DatabaseName);
    }
}
