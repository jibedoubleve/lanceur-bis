using System.Reflection;
using AutoMapper;
using Everything.Wrapper;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
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
using Lanceur.SharedKernel.Utils;
using Lanceur.SharedKernel.Web;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
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

    public static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddTransient<MainViewModel>()
                                .AddTransient<SettingsViewModel>()
                                .AddTransient<DoubloonsViewModel>()
                                .AddTransient<EmptyKeywordsModel>()
                                .AddTransient<HistoryViewModel>()
                                .AddTransient<KeywordsViewModel>()
                                .AddTransient<MostUsedViewModel>()
                                .AddTransient<PluginsViewModel>() 
                                .AddTransient<TrendsViewModel>() 
                                .AddTransient<ApplicationSettingsViewModel>(); 
        
    }

    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<Assembly>(_ => Assembly.GetExecutingAssembly());
        serviceCollection.AddTransient<IMemoryStorageService, MemoryStorageService>();
        serviceCollection.AddSingleton<IServiceProvider>(x => x);
        serviceCollection.AddTransient<IDbRepository, SQLiteRepository>();
        serviceCollection.AddTransient<IDbConnectionManager, DbMultiConnectionManager>();
        serviceCollection.AddTransient<IDbConnectionFactory, SQLiteProfiledConnectionFactory>();
        serviceCollection.AddTransient<IConnectionString, ConnectionString>();
        serviceCollection.AddTransient<IConversionService, AutoMapperConverter>();
        serviceCollection.AddTransient<IAsyncSearchService, SearchService>();
        serviceCollection.AddTransient<IStoreLoader, StoreLoader>();
        serviceCollection.AddTransient<IMacroManager, MacroManager>(s => 
            new(
                Assembly.GetAssembly(typeof(MultiMacro)), 
                s.GetService<ILogger<MacroManager>>(),
                s.GetService<IDbRepository>(),
                s.GetService<IServiceProvider>())
        );
        serviceCollection.AddTransient<ILoggerFactory, LoggerFactory>();
        serviceCollection.AddTransient<IThumbnailManager, ThumbnailManager>();
        serviceCollection.AddTransient<ISearchServiceOrchestrator, SearchServiceOrchestrator>();
        serviceCollection.AddTransient<IThumbnailManager, ThumbnailManager>();
        serviceCollection.AddTransient<IThumbnailRefresher, ThumbnailRefresher>();
        serviceCollection.AddTransient<IPackagedAppSearchService, PackagedAppSearchService>();
        serviceCollection.AddTransient<IFavIconManager, FavIconManager>();
        serviceCollection.AddTransient<IFavIconDownloader, FavIconDownloader>();
        serviceCollection.AddTransient<IPluginManager, PluginManager>();
        serviceCollection.AddTransient<IPluginStoreContext, PluginStoreContext>();
        serviceCollection.AddTransient<IEverythingApi, EverythingApi>();
        serviceCollection.AddTransient<IExecutionManager, ExecutionManager>();
        serviceCollection.AddTransient<IWildcardManager, ReplacementComposite>();
        serviceCollection.AddTransient<IClipboardService, WindowsClipboardService>();

        Conditional<Func<ILocalConfigRepository>> localConfigRepository =
            new(() => new MemoryLocalConfigRepository(), () => new JsonLocalConfigRepository());
        serviceCollection.AddSingleton(localConfigRepository.Value?.Invoke() ?? throw new ArgumentNullException(nameof(localConfigRepository)));

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
        var mapping = new MapperConfiguration(
            cfg =>
            {
                cfg.CreateMap<QueryResult, SelectableAliasQueryResult>();
                cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();

                cfg.CreateMap<string, DisplayQueryResult>()
                   .ConstructUsing(x => new($"@{x}@", "This is a macro", "LinkVariant"));
            }
        );
        var mapper = new Mapper(mapping);
        serviceCollection.AddSingleton<IMapper>(mapper);
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
        Log.Logger.Information("Logger configured...");
        return serviceCollection;
    }

    #endregion
}