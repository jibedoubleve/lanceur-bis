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
using Lanceur.Models;
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
using Serilog.Events;
using Serilog.Formatting.Compact;
using Splat;
using Splat.Serilog;
using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using Lanceur.Infra.SQLite.DataAccess;

namespace Lanceur;

public class Bootstrapper
{
    private static ServiceProvider _serviceCollection;

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
                                   new(x.Name, x.Notes, Get<ILoggerFactory>().GetLogger<SessionExecutableQueryResult>(),
                                       Get<IDbRepository>()))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        });
    }

    private static void RegisterLoggers()
    {
        var config = new LoggerConfiguration().MinimumLevel.Verbose()
                                              .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                              .Enrich.FromLogContext()
                                              .WriteTo.File(new CompactJsonFormatter(),
                                                            AppPaths.LogFilePath,
                                                            rollingInterval: RollingInterval.Day)
                                              .WriteTo.Console();
        _serviceCollection = new ServiceCollection().AddLogging(b => b.AddSerilog(config.CreateLogger()))
                                                    .BuildServiceProvider();
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

        l.Register<IStoreLoader>(() => new StoreLoader());
        l.Register<IAsyncSearchService>(() => new SearchService(Get<IStoreLoader>()));
        l.Register<ICmdlineManager>(() => new CmdlineManager());
        l.Register<IExecutionManager>(() => new ExecutionManager(Get<ILoggerFactory>(),
                                                                 Get<IWildcardManager>(),
                                                                 Get<IDbRepository>(),
                                                                 Get<ICmdlineManager>()));
        l.Register<ISearchServiceOrchestrator>(()=> new SearchServiceOrchestrator());
        l.Register<IDbRepository>(() =>
                                      new SQLiteRepository(Get<IDbConnectionManager>(),
                                                           Get<ILoggerFactory>(),
                                                           Get<IConversionService>()));
        l.Register<IDataDoctorRepository>(() => new SQLiteDataDoctorRepository(
                                              Get<IDbConnectionManager>(), 
                                              Get<ILoggerFactory>(),
                                              Get<IConversionService>()));
        l.Register<IWildcardManager>(() => new ReplacementComposite(Get<IClipboardService>()));
        l.Register<ICalculatorService>(() => new CodingSebCalculatorService());
        l.Register<IConversionService>(() => new AutoMapperConverter(Get<IMapper>()));
        l.Register<IClipboardService>(() => new WindowsClipboardService());
        l.RegisterLazySingleton<IMacroManager>(() => new MacroManager(Assembly.GetExecutingAssembly()));
        l.Register<IPluginManager>(() => new PluginManager(Get<ILoggerFactory>()));
        l.Register<IThumbnailRefresher>(() => new ThumbnailRefresher(Get<ILoggerFactory>(), Get<IPackagedAppSearchService>(), Get<IFavIconManager>()));
        l.Register<IThumbnailManager>(() => new ThumbnailManager(Get<ILoggerFactory>(), Get<IDbRepository>(), Get<IThumbnailRefresher>()));
        l.RegisterLazySingleton<IPackagedAppManager>(() => new PackagedAppManager());
        l.Register<IPackagedAppSearchService>(() => new PackagedAppSearchService(Get<ILoggerFactory>()));
        l.RegisterLazySingleton<IFavIconDownloader>(() => new FavIconDownloader(Get<ILoggerFactory>().GetLogger<IFavIconDownloader>()));
        l.Register<IFavIconManager>(() => new FavIconManager(Get<IPackagedAppSearchService>(), Get<IFavIconDownloader>(), Get<ILoggerFactory>()));

        // Formatters
        l.Register<IStringFormatter>(() => new DefaultStringFormatter());
        l.Register<IStringFormatter>(() => new LimitedStringLengthFormatter(), "limitedSize");

        // Plugins
        l.Register<IPluginManifestRepository>(() => new PluginStore());
        l.Register<IPluginInstaller>(() => new PluginButler(
                                         Get<ILoggerFactory>(),
                                         Get<IPluginValidationRule<PluginValidationResult, PluginManifest>>()));

        l.Register<IPluginUninstaller>(() => new PluginButler(
                                           Get<ILoggerFactory>(),
                                           Get<IPluginValidationRule<PluginValidationResult, PluginManifest>>()));

        l.Register<IPluginWebManifestLoader>(() => new PluginWebManifestLoader());
        l.Register<IPluginWebRepository>(() =>
                                             new PluginWebRepository(Get<IPluginManifestRepository>(),
                                                                     Get<IPluginWebManifestLoader>()));

        l.Register<IPluginValidationRule<PluginValidationResult, PluginManifest>>(() =>
                new PluginValidationRule(Get<IPluginManifestRepository>()));

        // SQLite
        l.Register(() => new SQLiteUpdater(Get<IDataStoreVersionManager>(),
                                           Get<ILoggerFactory>(),
                                           Get<IDataStoreUpdateManager>()));

        l.Register<IDbConnection>(() => new SQLiteConnection(Get<IConnectionString>().ToString()));
        l.Register<IDbConnectionFactory>(() => new SQLiteProfiledConnectionFactory(
                                             Get<IConnectionString>().ToString(), 
                                             Get<ILoggerFactory>())
        );
        l.Register<IDbConnectionManager>(() => new DbMultiConnectionManager(Get<SQLiteConnectionFactory>()));

        l.Register<IConnectionString>(() => new ConnectionString(Get<IDatabaseConfigRepository>()));

        l.Register((Func<IDataStoreVersionManager>)(() => new SQLiteVersionManager(Get<IDbConnectionManager>())));

        l.Register((Func<IDataStoreUpdateManager>)(() =>
                       new SQLiteDatabaseUpdateManager(
                           Get<IDataStoreVersionManager>(),
                           Get<IDbConnection>(),
                           ScriptRepository.Asm,
                           ScriptRepository.DbScriptEmbededResourcePattern)));
        
        // Logging
        l.UseSerilogFullLogger();
        l.Register(() => _serviceCollection?.GetService<ILoggerFactory>() ?? new DefaultLoggerFactory());
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
                log.Info("Add ViewModel {FullName} into IOC. Ctor has no ctor.", vmType.FullName);
                l.RegisterLazySingleton(() => Activator.CreateInstance(vmType));
                continue;
            }

            var ctor = vmType.GetConstructors()[0];
            var pCount = ctor.GetParameters().Length;

            log.Info("Add ViewModel {FullName} into IOC. Ctor has {pCount} parameter(s).", vmType.FullName, pCount);
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

        StaticLoggerFactory.GetLogger<Bootstrapper>().LogInformation("Settings DB path: {Settings}", stg);

        sqlite.Update(stg.ToString());
    }

    #endregion Methods
}