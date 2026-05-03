using Bewit;
using Bewit.Generation;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitGenerationServiceCollectionExtensions
{
    public static IServiceCollection AddBewitGeneration<T>(
        this IServiceCollection services)
        where T : notnull
    {
        string optionsName = typeof(T).FullName ?? typeof(T).Name;

        services.AddSingleton<IBewitTokenGenerator<T>>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<BewitOptions>>();
            BewitOptions bewitOptions = optionsMonitor.Get(optionsName);
            var options = Microsoft.Extensions.Options.Options.Create(bewitOptions);

            var cryptoService = new HmacSha256CryptographyService(bewitOptions.Secret);
            var nonceRepository = sp.GetRequiredKeyedService<INonceRepository>(optionsName);
            var variablesProvider = sp.GetRequiredService<IVariablesProvider>();

            return new BewitTokenGenerator<T>(
                options,
                cryptoService,
                nonceRepository,
                variablesProvider);
        });

        return services;
    }
}
