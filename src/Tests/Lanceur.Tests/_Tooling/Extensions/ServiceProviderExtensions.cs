using System.Data;
using System.Data.SQLite;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;
using ILogger = Castle.Core.Logging.ILogger;

namespace Lanceur.Tests.Tooling.Extensions;

public static class ServiceProviderExtensions
{
    #region Methods

    public static IServiceCollection AddApplicationSettings(this IServiceCollection serviceCollection)
    {
        var settings = Substitute.For<ISettingsFacade>();
        settings.Application.Returns(new AppConfig());

        serviceCollection.AddSingleton(settings);

        return serviceCollection;
    }

    public static IServiceCollection AddLogger<T>(this IServiceCollection serviceCollection, ITestOutputHelper outputHelper) => serviceCollection.AddSingleton<ILogger<T>>(new TestOutputHelperDecoratorForMicrosoftLogging<T>(outputHelper));

    public static IServiceCollection AddMemoryDb(this IServiceCollection serviceCollection, IDbConnectionManager connectionManager)
    {
        serviceCollection.AddMockSingleton<IConnectionString>()
                         .AddSingleton<IDbRepository, SQLiteRepository>()
                         .AddTransient<IDbConnection, SQLiteConnection>(sp => new(sp.GetService<IConnectionString>()!.ToString()))
                         .AddSingleton(connectionManager);
        return serviceCollection;
    }

    /// <summary>
    /// Configures the behaviour of a mocked singleton service within the specified <see cref="IServiceCollection"/>.
    /// Note that <see cref="IServiceProvider"/> is built at the moment this method is invoked, meaning any registrations
    /// added after this call will not be visible within the <paramref name="configurator"/> delegate.
    /// </summary>
    /// <typeparam name="T">The service type to be mocked and configured.</typeparam>
    /// <param name="serviceCollection">The service collection to which the configured singleton is added.</param>
    /// <param name="configurator">A delegate used to configure the mocked instance of <typeparamref name="T"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> containing the configured singleton service.</returns>
    public static IServiceCollection AddMockSingleton<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, T, T> configurator)
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

    /// <summary>
    /// Configures the view and logging services within the service provider.
    /// Note that the logger is a mock implementation.
    /// </summary>
    /// <param name="serviceCollection">The service collection to which services are added.</param>
    /// <typeparam name="T">The type representing the view component.</typeparam>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddView<T>(this IServiceCollection serviceCollection)
        where T : class
    {
        serviceCollection.AddSingleton<T>();
        serviceCollection.AddMockSingleton<ILogger<T>>();
        serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
        return serviceCollection;
    }

    #endregion
}