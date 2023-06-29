﻿using AutoMapper;
using Lanceur.Core.Formatters;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
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
using Lanceur.Models;
using Lanceur.Schedulers;
using Lanceur.Scripts;
using Lanceur.Ui;
using Lanceur.Utils;
using Lanceur.Utils.ConnectionStrings;
using Lanceur.Utils.PackagedApps;
using Lanceur.Views;
using ModernWpf;
using ReactiveUI;
using Splat;
using System;
using System.Data.SQLite;
using System.Reflection;

namespace Lanceur
{
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
                   .ConstructUsing(x => new DisplayQueryResult($"@{x}@", "This is a macro", "LinkVariant"));

                cfg.CreateMap<Session, SessionExecutableQueryResult>()
                   .ConstructUsing(x => new SessionExecutableQueryResult(x.Name, x.Notes, Get<IAppLoggerFactory>(), Get<IDbRepository>()))
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            });
        }

        private static void RegisterServices()
        {
            var l = Locator.CurrentMutable;

            Locator.CurrentMutable.RegisterConstant(new ReactiveUILogger() { Level = LogLevel.Debug }, typeof(ILogger));

            l.RegisterLazySingleton<IMapper>(() => new Mapper(GetAutoMapperCfg()));
            l.RegisterLazySingleton<IUserNotification>(() => new UserNotification());
            l.RegisterLazySingleton(() => new RoutingState());
            l.RegisterLazySingleton<IPluginStoreContext>(() => new PluginStoreContext());
            l.RegisterLazySingleton<IImageCache>(() => new ImageCache());
            l.RegisterLazySingleton<IDelay>(() => new Delay());
            l.RegisterLazySingleton<IAppRestart>(() => new AppRestart());

#if DEBUG
            l.Register<IDatabaseConfigRepository>(() => new MemoryDatabaseConfigRepository());
#else
            l.Register<IDatabaseConfigRepository>(() => new JsonDatabaseConfigRepository());
#endif
            l.Register<IAppConfigRepository>(() => new SQLiteAppConfigRepository(Get<SQLiteConnectionScope>()));
            l.RegisterLazySingleton<ISettingsFacade>(() => new SettingsFacade(Get<IDatabaseConfigRepository>(), Get<IAppConfigRepository>()));

            l.Register<ISchedulerProvider>(() => new RxAppSchedulerProvider());
            l.Register<IAppLoggerFactory>(() => new NLoggerFactory());
            l.Register<IStoreLoader>(() => new StoreLoader());
            l.Register<ISearchService>(() => new SearchService(Get<IStoreLoader>()));
            l.Register<ICmdlineManager>(() => new CmdlineManager());
            l.Register<IExecutionManager>(() => new ExecutionManager(Get<IAppLoggerFactory>(), Get<IWildcardManager>(), Get<IDbRepository>(), Get<ICmdlineManager>()));
            l.Register<IDbRepository>(() => new SQLiteRepository(Get<SQLiteConnectionScope>(), Get<IAppLoggerFactory>(), Get<IConvertionService>()));
            l.Register<IWildcardManager>(() => new ReplacementCollection(Get<IClipboardService>()));
            l.Register<ICalculatorService>(() => new CodingSebCalculatorService());
            l.Register<IConvertionService>(() => new AutoMapperConverter(Get<IMapper>()));
            l.Register<IClipboardService>(() => new WindowsClipboardService());
            l.Register<IMacroManager>(() => new MacroManager(Assembly.GetExecutingAssembly()));
            l.Register<IPluginManager>(() => new PluginManager(Get<IPluginStoreContext>()));
            l.Register<IThumbnailManager>(() => new WPFThumbnailManager(Get<IImageCache>()));
            l.Register<IPackagedAppManager>(() => new PackagedAppManager());
            l.Register<IPackagedAppValidator>(() => new PackagedAppValidator(Get<IPackagedAppManager>()));
            //Formatters
            l.Register<IStringFormatter>(() => new DefaultStringFormatter());

            l.Register(() => new SQLiteUpdater(Get<IDataStoreVersionManager>(), Get<IAppLoggerFactory>(), Get<IDataStoreUpdateManager>()));
            l.Register(() => new SQLiteConnection(Get<IConnectionString>().ToString()));
            l.Register(() => new SQLiteConnectionScope(Get<SQLiteConnection>()));
            l.Register<IConnectionString>(() => new ConnectionString(Get<IDatabaseConfigRepository>()));

            l.Register((Func<IDataStoreVersionManager>)(() => new SQLiteVersionManager(Get<SQLiteConnectionScope>())));

            l.Register((Func<IDataStoreUpdateManager>)(() =>
                new SQLiteDatabaseUpdateManager(
                    Get<IDataStoreVersionManager>(),
                    Get<SQLiteConnectionScope>(),
                    ScriptRepository.Asm,
                    ScriptRepository.DbScriptEmbededResourcePattern)
                )
            );
        }

        private static void RegisterViewModels()
        {
            var l = Locator.CurrentMutable;
            // Misc
            l.RegisterLazySingleton<INotification>(() => new ToastNotification());
            //Views
            l.RegisterLazySingleton(() => new MainViewModel());
            l.RegisterLazySingleton(() => new SettingsViewModel());
            l.RegisterLazySingleton(() => new KeywordsViewModel());
            l.RegisterLazySingleton(() => new SessionsViewModel());
            l.RegisterLazySingleton(() => new AppSettingsViewModel());
            l.RegisterLazySingleton(() => new DoubloonsViewModel());
            l.RegisterLazySingleton(() => new InvalidAliasViewModel());
            l.RegisterLazySingleton(() => new PluginsViewModel());
            // Statistics
            l.RegisterLazySingleton(() => new TrendsViewModel());
            l.RegisterLazySingleton(() => new MostUsedViewModel());
            l.RegisterLazySingleton(() => new HistoryViewModel()); ;
        }

        private static void RegisterViews()
        {
            var l = Locator.CurrentMutable;
            l.RegisterLazySingleton(() => new ConventionalViewLocator(), typeof(IViewFor));
            l.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
        }

        internal static void Initialise()
        {
            RegisterViews();
            RegisterViewModels();
            RegisterServices();

            var l = Locator.Current;
            var stg = l.GetService<IConnectionString>();
            var sqlite = l.GetService<SQLiteUpdater>();

            AppLogFactory.Get<Bootstrapper>().Trace($"Settings DB path: '{stg.ToString()}'");

            sqlite.Update(stg.ToString());
        }

        #endregion Methods
    }
}