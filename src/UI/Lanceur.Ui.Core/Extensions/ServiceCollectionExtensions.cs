using System.Data;
using System.Data.SQLite;
using System.Web.Bookmarks;
using System.Web.Bookmarks.Factories;
using Everything.Wrapper;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Core.Utils;
using Lanceur.Infra.Constants;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Infra.Win32.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Utils;
using Lanceur.SharedKernel.Web;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Lanceur.Ui.Core.Utils.Watchdogs;
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

    public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDatabaseConfigurationService, SQLiteDatabaseConfigurationService>();
        serviceCollection.AddTransient<ISettingsFacade, SettingsFacadeService>();
        return serviceCollection;
    }

    public static IServiceCollection AddLoggers(this IServiceCollection serviceCollection)
    {
        var conditional = new Conditional<LogEventLevel>(LogEventLevel.Verbose, LogEventLevel.Information);
        var levelSwitch = new LoggingLevelSwitch(conditional);

        serviceCollection.AddSingleton(levelSwitch);

        var loggerCfg = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch)
                                                 .Enrich.FromLogContext()
                                                 .Enrich.WithEnvironmentUserName()
                                                 .WriteTo.File(
                                                     new CompactJsonFormatter(),
                                                     Paths.LogFile,
                                                     rollingInterval: RollingInterval.Day
                                                 )
                                                 .WriteTo.Console();
#if DEBUG
        // For now, only seq is configured in my development machine and not anymore in AWS.
        loggerCfg.WriteTo.Seq(Paths.TelemetryUrl);
#endif
        Log.Logger = loggerCfg.CreateLogger();
        serviceCollection.AddLogging(builder => builder.AddSerilog(dispose: true))
                         .BuildServiceProvider();
        Log.Logger.Information("Logger configured");
        return serviceCollection;
    }

    public static IServiceCollection AddMapping(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMappingService, AutoMapperMappingService>();
        return serviceCollection;
    }

    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMemoryCache()
                         .AddSingleton<IServiceProvider>(x => x)
                         .AddSingleton<SQLiteUpdater>(
                             sp => new(
                                 sp.GetService<IDataStoreVersionService>(),
                                 sp.GetService<ILoggerFactory>(),
                                 sp.GetService<IDbConnection>(),
                                 ScriptRepository.Asm,
                                 ScriptRepository.DbScriptEmbededResourcePattern
                             )
                         )
                         .AddTransient<IDataStoreVersionService, SQLiteVersionService>()
                         .AddTransient<IAliasValidationService, AliasValidationService>()
                         .AddTransient<IAliasManagementService, AliasManagementService>()
                         .AddTransient<IMemoryStorageService, MemoryStorageService>()
                         .AddTransient<IAliasRepository, SQLiteAliasRepository>()
                         .AddTransient<IDbConnection, SQLiteConnection>(sp => new(sp.GetService<IConnectionString>()!.ToString()))
                         .AddTransient<IDbConnectionManager, DbMultiConnectionManager>()
                         .AddTransient<IDbConnectionFactory, SQLiteProfiledConnectionFactory>()
                         .AddTransient<IConnectionString, ConnectionString>()
                         .AddTransient<IMappingService, AutoMapperMappingService>()
                         .AddTransient<ISearchService, SearchService>()
                         .AddTransient<IStoreLoader, StoreLoader>()
                         .AddTransient<IMacroService, MacroService>()
                         .AddTransient<ILoggerFactory, LoggerFactory>()
                         .AddTransient<IThumbnailService, ThumbnailService>()
                         .AddTransient<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                         .AddTransient<IThumbnailService, ThumbnailService>()
                         .AddTransient<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddTransient<IFavIconService, FavIconService>()
                         .AddTransient<IFavIconDownloader, FavIconDownloader>()
                         .AddTransient<IEverythingApi, EverythingApi>()
                         .AddTransient<IExecutionService, ExecutionService>()
                         .AddTransient<IWildcardService, ReplacementComposite>()
                         .AddTransient<IClipboardService, WindowsClipboardService>()
                         .AddTransient<IReconciliationService, ReconciliationService>()
                         .AddTransient<IWatchdogBuilder, WatchdogBuilder>()
                         .AddTransient<IBookmarkRepositoryFactory, BookmarkRepositoryFactory>();

        ConditionalExecution.Set(
            serviceCollection,
            s => s.AddSingleton<IApplicationConfigurationService, MemoryApplicationConfigurationService>(),
            s => s.AddSingleton<IApplicationConfigurationService, JsonApplicationConfigurationService>()
        );

        return serviceCollection;
    }

    #endregion
}