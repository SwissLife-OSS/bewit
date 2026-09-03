using Bewit.Compatibility.V8.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Bewit;

public static class BewitV8MongoExtensions
{
    public static BewitBuilder UseV8MongoDb(
        this BewitBuilder builder,
        string purpose,
        Action<BewitV8MongoOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        var options = new BewitV8MongoOptions();
        configure?.Invoke(options);
        if (string.IsNullOrWhiteSpace(options.CollectionName))
        {
            throw new InvalidOperationException("The v8 collection name is required.");
        }
        builder.Services.AddSingleton<IBewitTokenStateStore>(provider =>
            new V8MongoTokenStateStore(
                provider.GetRequiredService<IMongoDatabase>(), options, purpose));
        return builder;
    }
}
