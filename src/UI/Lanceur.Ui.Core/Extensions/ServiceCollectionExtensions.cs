using System.Data;
using System.Data.SQLite;
using System.Reflection;
using Everything.Wrapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Core.Utils;
using Lanceur.Infra.Constants;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Plugins;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Infra.Win32.PackagedApp;
using Lanceur.Infra.Win32.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Utils;
using Lanceur.SharedKernel.Web;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Lanceur.Ui.Core.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IServiceProvider>(x => x)
                         .AddSingleton<SQLiteUpdater>()
                         .AddTransient<IDataStoreVersionManager, SQLiteVersionManager>()
                         .AddTransient<IDataStoreUpdateManager>(
                             sp =>  new SQLiteDatabaseUpdateManager(
                                 sp.GetService<IDataStoreVersionManager>(),
                                 sp.GetService<IDbConnection>(),
                                 ScriptRepository.Asm,
                                 ScriptRepository.DbScriptEmbededResourcePattern
                             )
                         )
                         .AddTransient<IAliasValidationService, AliasValidationService>()
                         .AddTransient<IAliasManagementService, AliasManagementService>()
                         .AddTransient(_ => Assembly.GetExecutingAssembly()) //TODO THIS IS WRONG, remove this line and fix the issue...
                         .AddTransient<IMemoryStorageService, MemoryStorageService>()
                         .AddTransient<IDbRepository, SQLiteRepository>()
                         .AddTransient<IDbConnection, SQLiteConnection>(sp => new(sp.GetService<IConnectionString>()!.ToString()))
                         .AddTransient<IDbConnectionManager, DbMultiConnectionManager>()
                         .AddTransient<IDbConnectionFactory, SQLiteProfiledConnectionFactory>()
                         .AddTransient<IConnectionString, ConnectionString>()
                         .AddTransient<IMappingService, AutoMapperMappingService>()
                         .AddTransient<ISearchService, SearchService>()
                         .AddTransient<IStoreLoader, StoreLoader>()
                         .AddTransient<IMacroManager, MacroManager>()
                         .AddTransient<ILoggerFactory, LoggerFactory>()
                         .AddTransient<IThumbnailManager, ThumbnailManager>()
                         .AddTransient<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                         .AddTransient<IThumbnailManager, ThumbnailManager>()
                         .AddTransient<IThumbnailRefresher, ThumbnailRefresher>()
                         .AddTransient<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddTransient<IFavIconManager, FavIconManager>()
                         .AddTransient<IFavIconDownloader, FavIconDownloader>()
                         .AddTransient<IPluginManager, PluginManager>()
                         .AddTransient<IPluginStoreContext, PluginStoreContext>()
                         .AddTransient<IEverythingApi, EverythingApi>()
                         .AddTransient<IExecutionManager, ExecutionManager>()
                         .AddTransient<IWildcardManager, ReplacementComposite>()
                         .AddTransient<IClipboardService, WindowsClipboardService>();

        ConditionalExecution.Set(
            serviceCollection,
            s => s.AddSingleton<ILocalConfigRepository, MemoryLocalConfigRepository>(),
            s => s.AddSingleton<ILocalConfigRepository, JsonLocalConfigRepository>()
        );

        return serviceCollection;
    }

    public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IAppConfigRepository, SQLiteAppConfigRepository>();
        serviceCollection.AddTransient<ISettingsFacade, SettingsFacade>();
        return serviceCollection;
    }

    public static IServiceCollection AddMapping(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMappingService, AutoMapperMappingService>();
        return serviceCollection;
    }

    public static IServiceCollection AddLoggers(this IServiceCollection serviceCollection)
    {
        var conditional = new Conditional<LogEventLevel>(LogEventLevel.Verbose, LogEventLevel.Information);
        var levelSwitch = new LoggingLevelSwitch(conditional);

        serviceCollection.AddSingleton(levelSwitch);

        Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch)
                                              .Enrich.FromLogContext()
                                              .Enrich.WithEnvironmentUserName()
                                              .WriteTo.File(
                                                  new CompactJsonFormatter(),
                                                  Paths.LogFile,
                                                  rollingInterval: RollingInterval.Day
                                              )
                                              .WriteTo.Console()
                                              .WriteTo.Seq(Paths.TelemetryUrl)
                                              .CreateLogger();
        serviceCollection.AddLogging(builder => builder.AddSerilog(dispose: true))
                         .BuildServiceProvider();
        Log.Logger.Information("Logger configured");
        return serviceCollection;
    }

    #endregion
}