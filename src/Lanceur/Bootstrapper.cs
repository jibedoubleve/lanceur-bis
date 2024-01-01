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
using Lanceur.Infra.Formatters;
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
using Lanceur.Models;
using Lanceur.Schedulers;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Web;
using Lanceur.Ui;
using Lanceur.Utils;
using Lanceur.Utils.ConnectionStrings;
using Lanceur.Utils.PackagedApps;
using Lanceur.Views;
using ReactiveUI;
using Serilog;
using Serilog.Events;
using Splat;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using Lanceur.Infra.Constants;
using Serilog.Formatting.Compact;
using Splat.Serilog;
using ILogger = Splat.ILogger;

namespace Lanceur;

public class Bootstrapper
{
    #region Methods

    private static T Get<T>() => Locator.Current.GetService<T>();

    private static IConfigurationProvider GetAutoMapperCfg()
    {
        return new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Session, SessionModel>();
            cfg.CreateMap<QueryResult, SelectableAliasQueryResult>();
            cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();

            cfg.CreateMap<string, DisplayQueryResult>()
               .ConstructUsing(x => new($"@{x}@", "This is a macro", "LinkVariant"));

            cfg.CreateMap<Session, SessionExecutableQueryResult>()
               .ConstructUsing(x =>
                                   new(x.Name, x.Notes, Get<IAppLoggerFactory>(),
                                       Get<IDbRepository>()))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        });
    }

    private static void RegisterLoggers()
    {
        Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Verbose()
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                     .Enrich.FromLogContext()
                     .WriteTo.File(new CompactJsonFormatter(), AppPaths.LogFilePath,
                                   rollingInterval: RollingInterval.Day)
                     .WriteTo.Console()
                     .CreateLogger();
    }

    private static void RegisterServices()
    {
        var l = Locator.CurrentMutable;

        l.RegisterLazySingleton<IMapper>(() => new Mapper(GetAutoMapperCfg()));
        l.RegisterLazySingleton<IUserNotification>(() => new UserNotification());
        l.RegisterLazySingleton(() => new RoutingState());
        l.RegisterLazySingleton<IPluginStoreContext>(() => new PluginStoreContext());
        l.RegisterLazySingleton<IDelay>(() => new Delay());
        l.RegisterLazySingleton<IAppRestart>(() => new AppRestart());

#if DEBUG
        l.Register<IDatabaseConfigRepository>(() => new MemoryDatabaseConfigRepository());
#else
        l.Register<IDatabaseConfigRepository>(() => new JsonDatabaseConfigRepository());
#endif
        l.Register<IAppConfigRepository>(() => new SQLiteAppConfigRepository(Get<IDbConnectionManager>()));
        l.RegisterLazySingleton<ISettingsFacade>(() =>
                                                     new SettingsFacade(Get<IDatabaseConfigRepository>(),
                                                                        Get<IAppConfigRepository>()));

        l.Register<ISchedulerProvider>(() => new RxAppSchedulerProvider());
        
        // Logging
        l.Register<IAppLoggerFactory>(() => new SerilogFactory());
        Locator.CurrentMutable.UseSerilogFullLogger();
        
        
        l.Register<IStoreLoader>(() => new StoreLoader());
        l.Register<ISearchService>(() => new SearchService(Get<IStoreLoader>()));
        l.Register<ICmdlineManager>(() => new CmdlineManager());
        l.Register<IExecutionManager>(() => new ExecutionManager(Get<IAppLoggerFactory>(),
                                                                 Get<IWildcardManager>(),
                                                                 Get<IDbRepository>(),
                                                                 Get<ICmdlineManager>()));
        l.Register<IDbRepository>(() =>
                                      new SQLiteRepository(Get<IDbConnectionManager>(),
                                                           Get<IAppLoggerFactory>(),
                                                           Get<IConvertionService>()));
        l.Register<IDataDoctorRepository>(() => new SQLiteDataDoctorRepository(Get<IDbConnectionManager>(),
                                                                               Get<IAppLoggerFactory>()));
        l.Register<IWildcardManager>(() => new ReplacementComposite(Get<IClipboardService>()));
        l.Register<ICalculatorService>(() => new CodingSebCalculatorService());
        l.Register<IConvertionService>(() => new AutoMapperConverter(Get<IMapper>()));
        l.Register<IClipboardService>(() => new WindowsClipboardService());
        l.RegisterLazySingleton<IMacroManager>(() => new MacroManager(Assembly.GetExecutingAssembly()));
        l.Register<IPluginManager>(() => new PluginManager(Get<IAppLoggerFactory>()));
        l.Register<IThumbnailRefresher>(() => new ThumbnailRefresher(Get<IAppLoggerFactory>(), Get<IPackagedAppSearchService>(), Get<IFavIconManager>()));
        l.Register<IThumbnailManager>(() => new ThumbnailManager(Get<IAppLoggerFactory>(), Get<IDbRepository>(), Get<IThumbnailRefresher>()));
        l.Register<IPackagedAppManager>(() => new PackagedAppManager());
        l.Register<IPackagedAppSearchService>(() => new PackagedAppSearchService());
        l.Register<IFavIconDownloader>(() => new FavIconDownloader());
        l.Register<IFavIconManager>(() => new FavIconManager(Get<IPackagedAppSearchService>(), Get<IFavIconDownloader>(), Get<IAppLoggerFactory>()));

        // Formatters
        l.Register<IStringFormatter>(() => new DefaultStringFormatter());

        // Plugins
        l.Register<IPluginManifestRepository>(() => new PluginStore());
        l.Register<IPluginInstaller>(() => new PluginButler(
                                         Get<IAppLoggerFactory>(),
                                         Get<IPluginValidationRule<PluginValidationResult, PluginManifest>>()));

        l.Register<IPluginUninstaller>(() => new PluginButler(
                                           Get<IAppLoggerFactory>(),
                                           Get<IPluginValidationRule<PluginValidationResult, PluginManifest>>()));

        l.Register<IPluginWebManifestLoader>(() => new PluginWebManifestLoader());
        l.Register<IPluginWebRepository>(() =>
                                             new PluginWebRepository(Get<IPluginManifestRepository>(),
                                                                     Get<IPluginWebManifestLoader>()));

        l.Register<IPluginValidationRule<PluginValidationResult, PluginManifest>>(() =>
                new PluginValidationRule(Get<IPluginManifestRepository>()));

        // SQLite
        l.Register(() => new SQLiteUpdater(Get<IDataStoreVersionManager>(),
                                           Get<IAppLoggerFactory>(),
                                           Get<IDataStoreUpdateManager>()));

        l.Register(() => new SQLiteConnection(Get<IConnectionString>().ToString()));
        l.Register<IDbConnectionManager>(() => new SQLiteMultiConnectionManager(Get<SQLiteConnection>()));

        l.Register<IConnectionString>(() => new ConnectionString(Get<IDatabaseConfigRepository>()));

        l.Register((Func<IDataStoreVersionManager>)(() => new SQLiteVersionManager(Get<IDbConnectionManager>())));

        l.Register((Func<IDataStoreUpdateManager>)(() =>
                       new SQLiteDatabaseUpdateManager(
                           Get<IDataStoreVersionManager>(),
                           Get<SQLiteConnection>(),
                           ScriptRepository.Asm,
                           ScriptRepository.DbScriptEmbededResourcePattern)));
    }

    private static void RegisterViewModels()
    {
        var l = Locator.CurrentMutable;
        var log = Locator.Current.GetService<ILogManager>().GetLogger<Bootstrapper>();

        // Misc
        l.RegisterLazySingleton<INotification>(() => new ToastNotification());

        // ViewModels
        var vmTypeCollection = (from type in Assembly.GetAssembly(typeof(MainViewModel))!.GetTypes()
                                where type.Name.EndsWith("ViewModel")
                                select type).ToList();

        foreach (var vmType in vmTypeCollection)
        {
            // https://stackoverflow.com/a/31348824/389529
            var ctorCollection = vmType.GetConstructors();

            if (ctorCollection.Length == 0)
            {
                log.Info("Add ViewModel '{FullName}' into IOC. Ctor has no ctor.", vmType.FullName);
                l.RegisterLazySingleton(() => Activator.CreateInstance(vmType));
                continue;
            }

            var ctor = vmType.GetConstructors()[0];
            var pCount = ctor.GetParameters().Length;

            log.Info("Add ViewModel '{FullName}' into IOC. Ctor has {pCount} parameter(s).", vmType.FullName, pCount);
            l.RegisterLazySingleton(() => ctor.Invoke(new object[pCount]), vmType);
        }
    }

    private static void RegisterViews()
    {
        var l = Locator.CurrentMutable;
        l.RegisterLazySingleton(() => new ConventionalViewLocator(), typeof(IViewFor));
        l.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
    }

    internal static void Initialise()
    {
        RegisterLoggers();
        RegisterViews();
        RegisterServices();
        RegisterViewModels();

        var l = Locator.Current;
        var stg = l.GetService<IConnectionString>();
        var sqlite = l.GetService<SQLiteUpdater>();

        AppLogFactory.Get<Bootstrapper>().Trace("Settings DB path: '{stg}'", stg);

        sqlite.Update(stg.ToString());
    }

    #endregion Methods
}