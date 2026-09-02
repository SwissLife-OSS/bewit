using Bewit;
using Bewit.Validation;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class BewitValidationServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton validation event handler and supplies the nonce repository
    /// configured for the payload type to its factory.
    /// </summary>
    public static IServiceCollection AddBewitTokenValidationEvents<T>(
        this IServiceCollection services,
        Func<IServiceProvider, INonceRepository, IBewitTokenValidationEvents<T>> factory)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(factory);

        string optionsName = typeof(T).FullName ?? typeof(T).Name;

        services.AddSingleton<IBewitTokenValidationEvents<T>>(sp =>
        {
            var nonceRepository = sp.GetRequiredKeyedService<INonceRepository>(optionsName);

            return factory(sp, nonceRepository);
        });

        return services;
    }

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
            var validationEvents = sp.GetServices<IBewitTokenValidationEvents<T>>();

            return new BewitTokenValidator<T>(
                options,
                cryptoService,
                nonceRepository,
                variablesProvider,
                validationEvents);
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
