using Bewit.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.AddOptions<BewitMongoOptions>()
            .Configure(configure)
            .ValidateDataAnnotations()
            .Validate(options => options.RecordExpireAfterDays > 0,
                "RecordExpireAfterDays must be positive.")
            .ValidateOnStart();
        builder.Services.TryAddSingleton<IMongoDatabase>(CreateDatabase);
        RegisterStore(builder.Services);
        return builder;
    }

    public static BewitBuilder UseMongoDb(
        this BewitBuilder builder,
        Func<IServiceProvider, IMongoDatabase> databaseFactory,
        Action<BewitMongoOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(databaseFactory);

        builder.Services.AddOptions<BewitMongoOptions>()
            .Configure(options =>
            {
                options.ConnectionString = "externally-provided";
                options.DatabaseName = "externally-provided";
                configure?.Invoke(options);
            })
            .ValidateDataAnnotations()
            .Validate(options => options.RecordExpireAfterDays > 0,
                "RecordExpireAfterDays must be positive.")
            .ValidateOnStart();
        builder.Services.TryAddSingleton(databaseFactory);
        RegisterStore(builder.Services);
        return builder;
    }

    private static void RegisterStore(IServiceCollection services)
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IBewitTokenStateStore, MongoBewitTokenStateStore>());
    }

    private static IMongoDatabase CreateDatabase(IServiceProvider serviceProvider)
    {
        BewitMongoOptions options = serviceProvider.GetRequiredService<IOptions<BewitMongoOptions>>().Value;
        MongoClientSettings settings = MongoClientSettings.FromConnectionString(options.ConnectionString);
        if (options.AuthType == MongoAuthType.Oidc)
        {
            if (options.OidcScopes is not { Count: > 0 })
            {
                throw new InvalidOperationException("OidcScopes are required for OIDC authentication.");
            }
            settings.Credential = MongoCredential.CreateOidcCredential(
                new BewitOidcCallback(options.OidcScopes));
        }
        return new MongoClient(settings).GetDatabase(options.DatabaseName);
    }
}
