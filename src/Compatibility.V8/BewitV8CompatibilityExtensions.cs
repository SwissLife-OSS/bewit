using Bewit.Compatibility.V8;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit;

public static class BewitV8CompatibilityExtensions
{
    public static BewitTokenBuilder<TPayload> AcceptV8Tokens<TPayload>(
        this BewitTokenBuilder<TPayload> builder,
        Action<BewitV8CompatibilityOptions> configure)
        where TPayload : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        var options = new BewitV8CompatibilityOptions();
        configure(options);
        if (string.IsNullOrWhiteSpace(options.Secret))
        {
            throw new InvalidOperationException("A v8 compatibility secret is required.");
        }
        if (options.MaximumTokenSizeBytes is < 256 or > 1024 * 1024)
        {
            throw new InvalidOperationException("MaximumTokenSizeBytes must be between 256 and 1048576.");
        }
        if (options.AcceptUntil is null)
        {
            throw new InvalidOperationException("AcceptUntil is required for v8 compatibility.");
        }

        options.PayloadTypeName ??= typeof(TPayload).FullName
            ?? throw new InvalidOperationException("The legacy payload type name must be specified.");
        builder.Services.AddSingleton(new BewitV8CompatibilityOptions<TPayload>(options));
        builder.Services.AddSingleton<IBewitTokenCodec<TPayload>, V8BewitTokenCodec<TPayload>>();
        return builder;
    }
}
