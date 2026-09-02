using Bewit;
using Bewit.Validation;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitValidationServiceCollectionExtensions
{
    public static IServiceCollection AddBewitValidation<T>(
        this IServiceCollection services)
        where T : notnull
    {
        string optionsName = typeof(T).FullName ?? typeof(T).Name;

        services.AddSingleton<IBewitTokenValidator<T>>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<BewitOptions>>();
            BewitOptions bewitOptions = optionsMonitor.Get(optionsName);
            var options = Microsoft.Extensions.Options.Options.Create(bewitOptions);

            var cryptoService = new HmacSha256CryptographyService(bewitOptions.Secret);
            var nonceRepository = sp.GetRequiredKeyedService<INonceRepository>(optionsName);
            var variablesProvider = sp.GetRequiredService<IVariablesProvider>();
            var observers = sp.GetServices<IBewitTokenValidationObserver<T>>();

            return new BewitTokenValidator<T>(
                options,
                cryptoService,
                nonceRepository,
                variablesProvider,
                observers);
        });

        return services;
    }

    public static IServiceCollection AddBewitValidation<T1, T2>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
    {
        services.AddBewitValidation<T1>();
        services.AddBewitValidation<T2>();

        return services;
    }

    public static IServiceCollection AddBewitValidation<T1, T2, T3>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        services.AddBewitValidation<T1>();
        services.AddBewitValidation<T2>();
        services.AddBewitValidation<T3>();

        return services;
    }

    public static IServiceCollection AddBewitValidation<T1, T2, T3, T4>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        services.AddBewitValidation<T1>();
        services.AddBewitValidation<T2>();
        services.AddBewitValidation<T3>();
        services.AddBewitValidation<T4>();

        return services;
    }

    public static IServiceCollection AddBewitValidation<T1, T2, T3, T4, T5>(
        this IServiceCollection services)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        services.AddBewitValidation<T1>();
        services.AddBewitValidation<T2>();
        services.AddBewitValidation<T3>();
        services.AddBewitValidation<T4>();
        services.AddBewitValidation<T5>();

        return services;
    }
}
