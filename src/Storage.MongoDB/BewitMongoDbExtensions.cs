using Bewit.Storage.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Authentication.Oidc;

namespace Bewit;

public static class BewitMongoDbExtensions
{
    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        Action<BewitMongoOptions> configure)
    {
        builder.UseNonceRepository(sp => CreateRepository(configure));

        return builder;
    }

    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        string connectionStringName,
        Action<BewitMongoOptions>? configure = null)
    {
        builder.UseNonceRepository(
            sp => CreateRepositoryFromConnectionString(sp, connectionStringName, configure));

        return builder;
    }

    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        Func<IServiceProvider, IMongoDatabase> databaseFactory,
        Action<BewitMongoOptions>? configure = null)
    {
        builder.UseNonceRepository(
            sp => CreateRepositoryFromDatabase(databaseFactory(sp), configure));

        return builder;
    }

    public static PayloadBuilder<T> UseMongoDb<T>(
        this PayloadBuilder<T> builder,
        Action<BewitMongoOptions> configure)
        where T : notnull
    {
        builder.UseNonceRepository(sp => CreateRepository(configure));

        return builder;
    }

    public static PayloadBuilder<T> UseMongoDb<T>(
        this PayloadBuilder<T> builder,
        string connectionStringName,
        Action<BewitMongoOptions>? configure = null)
        where T : notnull
    {
        builder.UseNonceRepository(
            sp => CreateRepositoryFromConnectionString(sp, connectionStringName, configure));

        return builder;
    }

    public static PayloadBuilder<T> UseMongoDb<T>(
        this PayloadBuilder<T> builder,
        Func<IServiceProvider, IMongoDatabase> databaseFactory,
        Action<BewitMongoOptions>? configure = null)
        where T : notnull
    {
        builder.UseNonceRepository(
            sp => CreateRepositoryFromDatabase(databaseFactory(sp), configure));

        return builder;
    }

    private static MongoNonceRepository CreateRepository(Action<BewitMongoOptions> configure)
    {
        var mongoOptions = new BewitMongoOptions();
        configure(mongoOptions);

        IMongoDatabase database = CreateDatabase(mongoOptions);

        return new MongoNonceRepository(
            database, Microsoft.Extensions.Options.Options.Create(mongoOptions));
    }

    private static MongoNonceRepository CreateRepositoryFromConnectionString(
        IServiceProvider sp,
        string connectionStringName,
        Action<BewitMongoOptions>? configure)
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

        return new MongoNonceRepository(
            database, Microsoft.Extensions.Options.Options.Create(mongoOptions));
    }

    private static MongoNonceRepository CreateRepositoryFromDatabase(
        IMongoDatabase database,
        Action<BewitMongoOptions>? configure)
    {
        var mongoOptions = new BewitMongoOptions
        {
            ConnectionString = "di-provided",
            DatabaseName = "di-provided"
        };

        configure?.Invoke(mongoOptions);

        return new MongoNonceRepository(
            database, Microsoft.Extensions.Options.Options.Create(mongoOptions));
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
