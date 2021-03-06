using AutoMapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Core.Utils;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Plugins;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Wildcards;
using Lanceur.Models;
using Lanceur.Ui;
using Lanceur.Utils;
using Lanceur.Views;
using ReactiveUI;
using Splat;
using Splat.NLog;
using System;
using System.Data.SQLite;
using System.Reflection;

namespace Lanceur
{
    internal class Bootstrapper
    {
        #region Fields

        private const string DbScriptEmbededResourcePattern = @"Lanceur\.SQL\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

        #endregion Fields

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
                   .ConstructUsing(x => new SessionExecutableQueryResult(x.Name, x.Notes, Get<ILogService>(), Get<IDataService>()))
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            });
        }

        private static void RegisterServices()
        {
            var l = Locator.CurrentMutable;

            //  then in your service locator initialisation
            l.UseNLogWithWrappingFullLogger();
            l.RegisterLazySingleton<IMapper>(() => new Mapper(GetAutoMapperCfg()));
            l.RegisterLazySingleton<IUserNotification>(() => new UserNotification());
            l.RegisterLazySingleton(() => new RoutingState());
            l.RegisterLazySingleton<IPluginStoreContext>(() => new PluginStoreContext());
            l.RegisterLazySingleton<IImageCache>(() => new ImageCache());

            l.Register<ILogService>(() => new NLogService());
            l.Register<IStoreLoader>(() => new StoreLoader());
            l.Register<ISearchService>(() => new SearchService(Get<IStoreLoader>()));
            l.Register<ICmdlineManager>(() => new CmdlineManager());
            l.Register<IDataService>(() => new SQLiteDataService(Get<SQLiteConnectionScope>(), Get<ILogService>(), Get<IExecutionManager>(), Get<IConvertionService>()));
            l.Register<IExecutionManager>(() => new ExecutionManager(Get<ILogService>(), Get<IWildcardManager>()));
            l.Register<IWildcardManager>(() => new ReplacementCollection(Get<IClipboardService>()));
            l.Register<IAppSettingsService>(() => new SQLiteAppSettingsService(Get<SQLiteConnectionScope>()));
            l.Register<ICalculatorService>(() => new CodingSebCalculatorService());
            l.Register<ISettingsService>(() => new JsonSettingsService());
            l.Register<IConvertionService>(() => new AutoMapperConverter(Get<IMapper>()));
            l.Register<IClipboardService>(() => new WindowsClipboardService());
            l.Register<IMacroManager>(() => new MacroManager(Assembly.GetExecutingAssembly()));
            l.Register<IPluginManager>(() => new PluginManager(Get<IPluginStoreContext>()));
            l.Register<IThumbnailManager>(() => new WPFThumbnailManager(Get<IImageCache>()));

            l.Register(() => new SQLiteDatabase(Get<IDataStoreVersionManager>(), Get<ILogService>(), Get<IDataStoreUpdateManager>()));
            l.Register(() => new SQLiteConnection(Get<IConnectionString>().ToString()));
            l.Register(() => new SQLiteConnectionScope(Get<SQLiteConnection>()));

            //#if DEBUG
            //            l.Register<IConnectionString>(() => new DebugConnectionString());
            //#else
            //            l.Register<IConnectionString>(() => new ConnectionString(Get<ISettingsService>()));
            //#endif
            l.Register<IConnectionString>(() => new ConnectionString(Get<ISettingsService>()));

            l.Register((Func<IDataStoreVersionManager>)(() => new SQLiteVersionManager(Get<SQLiteConnectionScope>())));
            l.Register((Func<IDataStoreUpdateManager>)(() =>
                new SQLiteDatabaseUpdateManager(Get<IDataStoreVersionManager>(), Get<SQLiteConnectionScope>(), Assembly.GetExecutingAssembly(), DbScriptEmbededResourcePattern)));
        }

        private static void RegisterViewModels()
        {
            var l = Locator.CurrentMutable;
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
            var sqlite = l.GetService<SQLiteDatabase>();

            LogService.Current.Trace($"Settings DB path: '{stg.ToString()}'");

            sqlite.Update(stg.ToString());
        }

        #endregion Methods
    }
}