using AutoMapper;
using Lanceur.Core.Formatters;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Core.Utils;
using Lanceur.Infra.Constants;
using Lanceur.Infra.Formatters;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Plugins;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Infra.Win32.PackagedApp;
using Lanceur.Infra.Win32.Restart;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Schedulers;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Web;
using Lanceur.Ui;
using Lanceur.Utils;
using Lanceur.Utils.ConnectionStrings;
using Lanceur.Utils.PackagedApps;
using Lanceur.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Serilog;
using Serilog.Formatting.Compact;
using Splat;
using Splat.Serilog;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using Everything.Wrapper;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.Utils;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lanceur;

public class Bootstrapper
{
    #region Fields

    private static readonly Conditional<Func<ILocalConfigRepository>> LocalConfigRepository =
        new(() => new MemoryLocalConfigRepository(null), () => new JsonLocalConfigRepository());

    private static ServiceProvider _serviceProvider;

    #endregion Fields

    #region Methods

    private static T Get<T>() => Locator.Current.GetService<T>();

    private static IConfigurationProvider GetAutoMapperCfg()
    {
        return new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<QueryResult, SelectableAliasQueryResult>();
                cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();

                cfg.CreateMap<string, DisplayQueryResult>()
                   .ConstructUsing(x => new($"@{x}@", "This is a macro", "LinkVariant"));
            }
        );
    }

    /// <remarks>Should be executed after <see cref="RegisterServices"/>.</remarks>
    private static void RegisterLoggers()
    {
        var lvlSwitch = Locator.Current.GetService<LoggingLevelSwitch>();
        Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(lvlSwitch)
                                              .Enrich.FromLogContext()
                                              .Enrich.WithEnvironmentUserName()
                                              .Filter.ByExcluding(Matching.WithProperty("SourceContext", "ReactiveUI.POCOObservableForProperty"))
                                              .WriteTo.File(
                                                  new CompactJsonFormatter(),
                                                  Paths.LogFile,
                                                  rollingInterval: RollingInterval.Day
                                              )
                                              .WriteTo.Console()
                                              .WriteTo.Seq(Paths.TelemetryUrl)
                                              .CreateLogger();
        _serviceProvider = new ServiceCollection().AddLogging(b => b.AddSerilog(Log.Logger))
                                                  .BuildServiceProvider();

        Locator.CurrentMutable.UseSerilogFullLogger(Log.Logger);
    }

    private static void RegisterServices()
    {
        var l = Locator.CurrentMutable;

        var conditional = new Conditional<LogEventLevel>(LogEventLevel.Verbose, LogEventLevel.Information);
        l.RegisterConstant(new LoggingLevelSwitch(conditional));

        l.RegisterLazySingleton<IMapper>(() => new Mapper(GetAutoMapperCfg()));
        l.RegisterLazySingleton<IUserNotification>(() => new UserNotification());
        l.RegisterLazySingleton(() => new RoutingState());
        l.RegisterLazySingleton<IPluginStoreContext>(() => new PluginStoreContext());
        l.RegisterLazySingleton<IDelay>(() => new Delay());
        l.RegisterLazySingleton<IAppRestart>(() => new AppRestart());
        l.Register(() => LocalConfigRepository.Value?.Invoke());
        l.Register<IAppConfigRepository>(() => new SQLiteAppConfigRepository(Get<IDbConnectionManager>()));
        l.RegisterLazySingleton<ISettingsFacade>(
            () =>
                new SettingsFacade(
                    Get<ILocalConfigRepository>(),
                    Get<IAppConfigRepository>()
                )
        );

        l.Register<ISchedulerProvider>(() => new RxAppSchedulerProvider());
        l.Register<IEverythingApi>(() => new EverythingApi());
        l.Register<IStoreLoader>(() => new StoreLoader(null, Get<SearchServiceOrchestrator>(), Get<ServiceProvider>()));
        l.Register<ISearchService>(() => new SearchService(Get<IStoreLoader>()));
        l.Register<IExecutionManager>(
            () => new ExecutionManager(
                Get<ILoggerFactory>(),
                Get<IWildcardManager>(),
                Get<IDbRepository>()
            )
        );
        l.Register<ISearchServiceOrchestrator>(() => new SearchServiceOrchestrator());
        l.Register<IDbRepository>(
            () =>
                new SQLiteRepository(
                    Get<IDbConnectionManager>(),
                    Get<ILoggerFactory>(),
                    Get<IConversionService>()
                )
        );
        l.Register<IDataDoctorRepository>(
            () => new SQLiteDataDoctorRepository(
                Get<IDbConnectionManager>(),
                Get<ILoggerFactory>(),
                Get<IConversionService>()
            )
        );
        l.Register<IWildcardManager>(() => new ReplacementComposite(Get<IClipboardService>()));
        l.Register<ICalculatorService>(() => new CodingSebCalculatorService());
        l.Register<IConversionService>(() => new AutoMapperConverter(Get<IMapper>()));
        l.Register<IClipboardService>(() => new WindowsClipboardService());
        l.RegisterLazySingleton<IMacroManager>(() => new MacroManager(Assembly.GetExecutingAssembly(), null, null, null));
        l.Register<IPluginManager>(() => new PluginManager(Get<ILoggerFactory>()));
        l.Register<IThumbnailRefresher>(() => new ThumbnailRefresher(Get<ILoggerFactory>(), Get<IPackagedAppSearchService>(), Get<IFavIconManager>()));
        l.Register<IThumbnailManager>(() => new ThumbnailManager(Get<ILoggerFactory>(), Get<IDbRepository>(), Get<IThumbnailRefresher>()));
        l.RegisterLazySingleton<IPackagedAppManager>(() => new PackagedAppManager());
        l.Register<IPackagedAppSearchService>(() => new PackagedAppSearchService(Get<ILoggerFactory>()));
        l.RegisterLazySingleton<IFavIconDownloader>(() => new FavIconDownloader(null));
        l.Register<IFavIconManager>(
            () => new FavIconManager(
                Get<IPackagedAppSearchService>(),
                Get<IFavIconDownloader>(),
                Get<ILoggerFactory>()
            )
        );

        // Formatters
        l.Register<IStringFormatter>(() => new DefaultStringFormatter());
        l.Register<IStringFormatter>(() => new LimitedStringLengthFormatter(), "limitedSize");

        // Plugins
        l.Register<IPluginManifestRepository>(() => new PluginStore());
        l.Register<IPluginInstaller>(
            () => new PluginButler(
                Get<ILoggerFactory>(),
                Get<IPluginValidationRule<PluginValidationResult, PluginManifest>>()
            )
        );

        l.Register<IPluginUninstaller>(
            () => new PluginButler(
                Get<ILoggerFactory>(),
                Get<IPluginValidationRule<PluginValidationResult, PluginManifest>>()
            )
        );

        l.Register<IPluginWebManifestLoader>(() => new PluginWebManifestLoader());
        l.Register<IPluginWebRepository>(
            () =>
                new PluginWebRepository(
                    Get<IPluginManifestRepository>(),
                    Get<IPluginWebManifestLoader>()
                )
        );

        l.Register<IPluginValidationRule<PluginValidationResult, PluginManifest>>(
            () =>
                new PluginValidationRule(Get<IPluginManifestRepository>())
        );

        // SQLite
        l.Register(
            () => new SQLiteUpdater(
                Get<IDataStoreVersionManager>(),
                Get<ILoggerFactory>(),
                Get<IDataStoreUpdateManager>()
            )
        );

        l.Register<IDbConnection>(() => new SQLiteConnection(Get<IConnectionString>().ToString()));
        l.Register<IDbConnectionFactory>(
            () => new SQLiteProfiledConnectionFactory(
                Get<IConnectionString>(),
                Get<ILoggerFactory>().GetLogger<SQLiteProfiledConnectionFactory>()
            )
        );
        l.Register<IDbConnectionManager>(() => new DbMultiConnectionManager(Get<IDbConnectionFactory>()));

        l.Register<IConnectionString>(() => new ConnectionString(Get<ILocalConfigRepository>()));

        l.Register((Func<IDataStoreVersionManager>)(() => new SQLiteVersionManager(Get<IDbConnectionManager>())));

        l.Register(
            (Func<IDataStoreUpdateManager>)(() =>
                new SQLiteDatabaseUpdateManager(
                    Get<IDataStoreVersionManager>(),
                    Get<IDbConnection>(),
                    ScriptRepository.Asm,
                    ScriptRepository.DbScriptEmbededResourcePattern
                ))
        );

        l.Register(() => _serviceProvider?.GetService<ILoggerFactory>() ?? new DefaultLoggerFactory());
    }

    private static void RegisterViewModels()
    {
        var l = Locator.CurrentMutable;
        var log = Locator.Current.GetService<ILoggerFactory>().GetLogger<Bootstrapper>();

        // Misc
        l.RegisterLazySingleton<INotification>(() => new ToastNotification());

        // ViewModels
        var vmTypeCollection = Assembly.GetAssembly(typeof(MainViewModel))!.GetTypes()
                                       .Where(type => type.Name.EndsWith("ViewModel"))
                                       .ToList();

        foreach (var vmType in vmTypeCollection)
        {
            // https://stackoverflow.com/a/31348824/389529
            var ctorCollection = vmType.GetConstructors();

            if (ctorCollection.Length == 0)
            {
                log.LogDebug("Add ViewModel {FullName} into IOC. Ctor has no ctor", vmType.FullName);
                l.RegisterLazySingleton(() => Activator.CreateInstance(vmType));
                continue;
            }

            var ctor = vmType.GetConstructors()[0];
            var pCount = ctor.GetParameters().Length;

            log.LogDebug("Add ViewModel {FullName} into IOC. Ctor has {Count} parameter(s)", vmType.FullName, pCount);
            l.RegisterLazySingleton(() => ctor.Invoke(new object[pCount]), vmType);
        }
    }

    private static void RegisterViews() => Locator.CurrentMutable
                                                  .RegisterViewsForViewModels(Assembly.GetCallingAssembly());

    internal static void Initialise()
    {
        RegisterServices();
        RegisterLoggers();
        RegisterViews();
        RegisterViewModels();

        var l = Locator.Current;
        var stg = l.GetService<IConnectionString>();
        var sqlite = l.GetService<SQLiteUpdater>();

        Locator.Current.GetLogger<Bootstrapper>().LogInformation("Settings DB path: {Settings}", stg);

        sqlite.Update(stg.ToString());
    }

    public static void TearDown() => Log.CloseAndFlush();

    #endregion Methods
}