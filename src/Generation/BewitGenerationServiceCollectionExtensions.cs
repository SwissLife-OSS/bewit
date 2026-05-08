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

    public static IServiceCollection AddBewitGeneration<T1, T2>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
    {
        services.AddBewitGeneration<T1>();
        services.AddBewitGeneration<T2>();

        return services;
    }

    public static IServiceCollection AddBewitGeneration<T1, T2, T3>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        services.AddBewitGeneration<T1>();
        services.AddBewitGeneration<T2>();
        services.AddBewitGeneration<T3>();

        return services;
    }

    public static IServiceCollection AddBewitGeneration<T1, T2, T3, T4>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        services.AddBewitGeneration<T1>();
        services.AddBewitGeneration<T2>();
        services.AddBewitGeneration<T3>();
        services.AddBewitGeneration<T4>();

        return services;
    }

    public static IServiceCollection AddBewitGeneration<T1, T2, T3, T4, T5>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        services.AddBewitGeneration<T1>();
        services.AddBewitGeneration<T2>();
        services.AddBewitGeneration<T3>();
        services.AddBewitGeneration<T4>();
        services.AddBewitGeneration<T5>();

        return services;
    }
}
