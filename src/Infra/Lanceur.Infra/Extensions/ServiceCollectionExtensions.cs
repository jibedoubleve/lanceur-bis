using System.Web.Bookmarks;
using System.Web.Bookmarks.Factories;
using Everything.Wrapper;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Services;
using Lanceur.Infra.LuaScripting;
using Lanceur.Infra.Services;
using Lanceur.Infra.Wildcards;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfraServices(this IServiceCollection serviceCollection)
    {

        serviceCollection.AddTransient<IAliasValidationService, AliasValidationService>()
                         .AddTransient<IAliasManagementService, AliasManagementService>()
                         .AddTransient<ISearchService, SearchService>()
                         .AddTransient<IMacroAliasExpanderService, MacroAliasExpanderService>()
                         .AddTransient<IFavIconService, FavIconService>()
                         .AddSingleton<IFavIconDownloader, FavIconDownloader>()
                         .AddTransient<IExecutionService, ExecutionService>()
                         .AddTransient<IWildcardService, ReplacementComposite>()
                         .AddTransient<IReconciliationService, ReconciliationService>()
                         .AddTransient<IFeatureFlagService, FeatureFlagService>()
                         .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                         .AddSingleton<ILuaManager, LuaManager>()
                         .AddTransient<IStoreShortcutService, StoreShortcutService>()
                         .AddSingleton<IFavIconHttpClient, FavIconHttpClient>()
                         .AddTransient<IEverythingApi, EverythingApi>()
                         .AddTransient<IBookmarkRepositoryFactory, BookmarkRepositoryFactory>();
        return serviceCollection;
    }
}