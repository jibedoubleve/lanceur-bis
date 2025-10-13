using System.Data;
using System.Data.SQLite;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Tools.Extensions;

public static class ServiceProviderExtensions
{
    #region Methods

    public static IServiceCollection AddApplicationSettings(
        this IServiceCollection serviceCollection,
        Action<ISettingsFacade>? setupAction = null
    )
    {
        var settings = Substitute.For<ISettingsFacade>();
        settings.Application.Returns(new NoCacheDatabaseConfiguration());

        serviceCollection.AddSingleton(settings);
        setupAction?.Invoke(settings);
        return serviceCollection;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection serviceCollection,
        IDbConnectionManager connectionManager
    )
    {
        serviceCollection.AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                         .AddTransient<IDbConnection, SQLiteConnection>(sp
                             => new(sp.GetService<IConnectionString>()!.ToString())
                         )
                         .AddSingleton(connectionManager);
        return serviceCollection;
    }

    public static IServiceCollection AddLoggingForTests<T>(
        this IServiceCollection serviceCollection,
        ITestOutputHelper outputHelper
    ) => serviceCollection.AddSingleton<ILogger<T>>(new TestOutputHelperDecoratorForMicrosoftLogging<T>(outputHelper));


    /// <summary>
    ///     Configures the behaviour of a mocked singleton service within the specified <see cref="IServiceCollection" />.
    ///     Note that <see cref="IServiceProvider" /> is built at the moment this method is invoked, meaning any registrations
    ///     added after this call will not be visible within the <paramref name="configurator" /> delegate.
    /// </summary>
    /// <typeparam name="T">The service type to be mocked and configured.</typeparam>
    /// <param name="serviceCollection">The service collection to which the configured singleton is added.</param>
    /// <param name="configurator">A delegate used to configure the mocked instance of <typeparamref name="T" />.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> containing the configured singleton service.</returns>
    public static IServiceCollection AddMockSingleton<T>(
        this IServiceCollection serviceCollection,
        Func<IServiceProvider, T, T> configurator
    )
        where T : class
    {
        var substitute = Substitute.For<T>();
        var service = configurator(serviceCollection.BuildServiceProvider(), substitute);
        serviceCollection.AddSingleton(service);
        return serviceCollection;
    }

    public static IServiceCollection AddMockSingleton<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        serviceCollection.AddSingleton(Substitute.For<T>());
        return serviceCollection;
    }

    #endregion
}