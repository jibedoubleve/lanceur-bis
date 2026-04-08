using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Caching;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
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

        var minLogLevel = settingsProvider.Value.Logging.GetMinimumLogLevel();
        var logging = settingsProvider.Value.Logging;

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

    public static IServiceCollection AddTrackedMemoryCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.Decorate<IMemoryCache, TrackedMemoryCache>();

        return services;
    }

    public static IServiceCollection AddUiCoreServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IGithubService, GithubService>()
                         .AddTransient<IClipboardService, ClipboardService>()
                         .AddTransient<IWatchdogBuilder, WatchdogBuilder>()
                         .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                         .AddSingleton<IEnigma, Enigma>()
                         .AddHttpClient();
        return serviceCollection;
    }

    #endregion
}