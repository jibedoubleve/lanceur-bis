using System.Data;
using System.Data.SQLite;
using System.Web.Bookmarks;
using System.Web.Bookmarks.Factories;
using Everything.Wrapper;
using Lanceur.Core.Constants;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Mappers;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Core.Utils;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Infra.Win32.Services;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Caching;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Utils;
using Lanceur.SharedKernel.Web;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Lanceur.Ui.Core.Utils.Watchdogs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;
using Serilog.Sinks.Grafana.Loki;

namespace Lanceur.Ui.Core.Extensions;

public static class ServiceCollectionExtensions
{
    #region Methods

    public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDatabaseConfigurationService, SQLiteDatabaseConfigurationService>();
        serviceCollection.AddTransient<ISettingsFacade, SettingsFacadeService>();
        serviceCollection.AddTransient<IGithubService, GithubService>();
        return serviceCollection;
    }

    public static void AddLoggers(
        this IServiceCollection serviceCollection,
        HostBuilderContext context,
        ServiceProvider serviceProvider
    )
    {
        var settingsFacadeService = serviceProvider.GetRequiredService<SettingsFacadeService>();
        var logEventLevel = new Conditional<LogEventLevel>(
            LogEventLevel.Debug,
            settingsFacadeService.GetMinimumLogLevel()
        );
        var levelSwitch = new LoggingLevelSwitch(logEventLevel);
        var telemetry = serviceProvider.GetRequiredService<ISettingsFacade>().Local.Telemetry;

        serviceCollection.AddSingleton(levelSwitch);

        var loggerCfg = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch)
                                                 .Enrich.FromLogContext()
                                                 .Enrich.WithEnvironmentUserName()
                                                 .WriteTo.Console();

        ConditionalExecution.Execute(
            ConfigureLogForDebug,
            ConfigureLogForRelease
        );

        serviceCollection.AddLogging(builder => builder.AddSerilog(dispose: true));
        Log.Logger = loggerCfg.CreateLogger();

        return;

        void ConfigureLogForDebug()
        {
            if (telemetry.IsSeqEnabled)
            {
                // For now, only seq is configured in my development machine and not anymore in AWS.
                var apiKey = context.Configuration["SEQ_LANCEUR"];
                if (apiKey is null)
                    throw new NotSupportedException(
                        "Api key not found. Create a environment variable 'SEQ_LANCEUR' with the api key"
                    );

                loggerCfg.WriteTo.Seq(
                    Paths.TelemetryUrlSeq,
                    apiKey: apiKey
                );
            }

            if (telemetry.IsLokiEnabled)
                loggerCfg.WriteTo.GrafanaLoki(
                    Paths.TelemetryUrlLoki,
                    [
                        new()  { Key = "app", Value = "lanceur-bis" },
                        new()  { Key = "env", Value = new Conditional<string>("dev", "prod") }
                    ]
                );
        }

        void ConfigureLogForRelease()
        {
            if (telemetry.IsClefEnabled)
                // Clef file, easier to import into SEQ
                loggerCfg.WriteTo.File(
                    new CompactJsonFormatter(),
                    Paths.ClefLogFile,
                    rollingInterval: RollingInterval.Day
                );

            // Raw log file, easier to read
            loggerCfg.WriteTo.File(
                new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"),
                Paths.RawLogFile,
                rollingInterval: RollingInterval.Day
            );
        }
    }

    public static IServiceCollection AddMapping(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMappingService, MappingService>();
        return serviceCollection;
    }

    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                         .AddSingleton<IServiceProvider>(x => x)
                         .AddSingleton<SQLiteUpdater>(sp => new(
                                 sp.GetService<IDataStoreVersionService>(),
                                 sp.GetService<ILoggerFactory>(),
                                 sp.GetService<IDbConnection>(),
                                 ScriptRepository.Asm,
                                 ScriptRepository.DbScriptEmbeddedResourcePattern
                             )
                         )
                         .AddTransient<IDataStoreVersionService, SQLiteVersionService>()
                         .AddTransient<IAliasValidationService, AliasValidationService>()
                         .AddTransient<IAliasManagementService, AliasManagementService>()
                         .AddTransient<IClipboardService, ClipboardService>()
                         .AddTransient<IAliasRepository, SQLiteAliasRepository>()
                         .AddTransient<IDbConnection, SQLiteConnection>(sp => new(
                                 sp.GetService<IConnectionString>()!
                                   .ToString()
                             )
                         )
                         .AddTransient<IDbConnectionManager, DbMultiConnectionManager>()
                         .AddTransient<IDbConnectionFactory, SQLiteProfiledConnectionFactory>()
                         .AddTransient<IConnectionString, ConnectionString>()
                         .AddTransient<IMappingService, MappingService>()
                         .AddTransient<ISearchService, SearchService>()
                         .AddTransient<IStoreLoader, StoreLoader>()
                         .AddTransient<IMacroService, MacroService>()
                         .AddTransient<ILoggerFactory, LoggerFactory>()
                         .AddTransient<IThumbnailService, ThumbnailService>()
                         .AddTransient<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                         .AddTransient<IThumbnailService, ThumbnailService>()
                         .AddTransient<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddTransient<IFavIconService, FavIconService>()
                         .AddSingleton<IFavIconDownloader, FavIconDownloader>()
                         .AddTransient<IEverythingApi, EverythingApi>()
                         .AddTransient<IExecutionService, ExecutionService>()
                         .AddTransient<IWildcardService, ReplacementComposite>()
                         .AddTransient<IReconciliationService, ReconciliationService>()
                         .AddTransient<IWatchdogBuilder, WatchdogBuilder>()
                         .AddTransient<IFeatureFlagService, SQLiteFeatureFlagService>()
                         .AddTransient<IBookmarkRepositoryFactory, BookmarkRepositoryFactory>()
                         .AddTransientConditional<IProcessLauncher, ProcessLauncherLogger, ProcessLauncherWin32>()
                         .AddSingletonConditional<IApplicationConfigurationService,
                             MemoryApplicationConfigurationService, JsonApplicationConfigurationService>()
                         .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                         .AddSingleton<ILuaManager, LuaManager>()
                         .AddSingleton<IEnigma, Enigma>();

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