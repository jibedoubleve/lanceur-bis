using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Views;
using Microsoft.Reactive.Testing;
using NSubstitute;

namespace Lanceur.Tests.Utils
{
    internal static class Builder
    {
        #region Methods

        public static IExecutionManager BuildExecutionManager(IAppLoggerFactory logService = null, IWildcardManager wildcardManager = null, IDataService dataService = null, ICmdlineManager cmdlineManager = null)
        {
            logService ??= Substitute.For<IAppLoggerFactory>();
            wildcardManager ??= Substitute.For<IWildcardManager>();
            dataService ??= Substitute.For<IDataService>();
            cmdlineManager ??= new CmdlineManager();//Substitute.For<ICmdlineManager>();
            return new ExecutionManager(logService, wildcardManager, dataService, cmdlineManager);
        }

        public static MainViewModel BuildMainViewModel(TestScheduler scheduler, ISearchService searchService = null, IExecutionManager executor = null)
        {
            return new MainViewModel(
                uiThread: scheduler,
                poolThread: scheduler,
                logFactory: ServiceFactory.LogService,
                searchService: searchService ?? ServiceFactory.SearchService,
                cmdlineService: new CmdlineManager(),
                executor: executor ?? ServiceFactory.ExecutionManager
            );
        }

        #endregion Methods
    }
}