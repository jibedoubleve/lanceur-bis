using System.Data;
using System.Data.SQLite;
using System.Web.Bookmarks;
using System.Web.Bookmarks.Factories;
using Everything.Wrapper;
using Lanceur.Core.Constants;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Wildcards;
using Lanceur.Infra.Win32.Helpers;
using Lanceur.Infra.Win32.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Caching;
using Lanceur.SharedKernel.IoC;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;

namespace Lanceur.Ui.Core.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddLoggers(
        this IServiceCollection serviceCollection
    )
    {
        var settingsProvider = SettingsProviderFactory.GetInfrastructureSettingsProvider();
        settingsProvider.Load();

        var minLogLevel = settingsProvider.Current.Logging.GetMinimumLogLevel();
        var logging = settingsProvider.Current.Logging;

        var logEventLevel = new Conditional<LogEventLevel>(
            LogLevelUtil.GetLevel(),
            minLogLevel
        );
        var levelSwitch = new LoggingLevelSwitch(logEventLevel);

        serviceCollection.AddSingleton(levelSwitch);

        var loggerCfg = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch)
                                                 .Enrich.FromLogContext()
                                                 .Enrich.WithEnvironmentUserName()
                                                 .WriteTo.Console();


        ConditionalExecution.Execute(
            () => ConfigureLog(Paths.DebugClefLogFile, Paths.DebugRawLogFile),
            () => ConfigureLog(Paths.ClefLogFile, Paths.RawLogFile)
        );

        serviceCollection.AddLogging(builder => builder.ClearProviders()
                                                       .AddSerilog(dispose: true));
        Log.Logger = loggerCfg.CreateLogger();

        return serviceCollection;

        void ConfigureLog(string clefFile, string logFile)
        {
            if (logging.IsClefEnabled)
                // Clef file, easier to import into SEQ
            {
                loggerCfg.WriteTo.File(
                    new CompactJsonFormatter(),
                    clefFile,
                    rollingInterval: RollingInterval.Day
                );
            }

            // Raw log file, easier to read
            loggerCfg.WriteTo.File(
                new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"),
                logFile,
                rollingInterval: RollingInterval.Day
            );
        }
    }

    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddConfigurationSections()
                         .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                         .AddSingleton<IServiceProvider>(x => x)
                         .AddSingleton<SQLiteUpdater>(sp => new SQLiteUpdater(
                                 sp.GetService<IDataStoreVersionService>()!,
                                 sp.GetService<ILoggerFactory>()!,
                                 sp.GetService<IDbConnection>()!,
                                 ScriptRepository.Asm,
                                 ScriptRepository.DbScriptEmbeddedResourcePattern
                             )
                         )
                         .AddTransient<IDataStoreVersionService, SQLiteVersionService>()
                         .AddTransient<IAliasValidationService, AliasValidationService>()
                         .AddTransient<IAliasManagementService, AliasManagementService>()
                         .AddTransient<IClipboardService, ClipboardService>()
                         .AddTransient<IAliasRepository, SQLiteAliasRepository>()
                         .AddTransient<IDbConnection, SQLiteConnection>(sp
                             => new SQLiteConnection(sp.GetService<IConnectionString>()!.ToString())
                         )
                         .AddTransient<IDbConnectionManager, DbMultiConnectionManager>()
                         .AddTransient<IDbConnectionFactory, SQLiteProfiledConnectionFactory>()
                         .AddTransient<IConnectionString, ConnectionString>()
                         .AddTransient<ISearchService, SearchService>()
                         .AddTransient<IMacroAliasExpanderService, MacroAliasExpanderService>()
                         .AddTransient<ILoggerFactory, LoggerFactory>()
                         .AddTransient<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                         .AddTransient<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddTransient<IFavIconService, FavIconService>()
                         .AddSingleton<IFavIconDownloader, FavIconDownloader>()
                         .AddTransient<IEverythingApi, EverythingApi>()
                         .AddTransient<IExecutionService, ExecutionService>()
                         .AddTransient<IWildcardService, ReplacementComposite>()
                         .AddTransient<IReconciliationService, ReconciliationService>()
                         .AddTransient<IWatchdogBuilder, WatchdogBuilder>()
                         .AddTransient<IFeatureFlagService, FeatureFlagService>()
                         .AddTransient<IBookmarkRepositoryFactory, BookmarkRepositoryFactory>()
                         .AddTransientConditional<IProcessLauncher, ProcessLauncherNoOp, ProcessLauncherWin32>()
                         .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                         .AddSingleton<ILuaManager, LuaManager>()
                         .AddSingleton<IEnigma, Enigma>()
                         .AddHttpClient()
                         .AddThumbnailStrategies()
                         .AddStaThreadRunner()
                         .AddTransient<IThumbnailService, ThumbnailService>()
                         .AddTransient<ISteamLibraryService, SteamLibraryService>()
                         .AddTransient<IStoreShortcutService, StoreShortcutService>()
                         .AddTransient<IFeatureFlagService, FeatureFlagService>()
                         .AddTransient<IFeatureFlagRepository, SQLiteFeatureFlagRepository>()
                         .AddSingleton<IFavIconHttpClient, FavIconHttpClient>();

        return serviceCollection;
    }

    public static IServiceCollection AddTrackedMemoryCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.Decorate<IMemoryCache, TrackedMemoryCache>();

        return services;
    }

    #endregion
}