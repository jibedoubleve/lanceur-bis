using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Tests.Utils.ReservedAliases;
using Lanceur.Views;
using Microsoft.Reactive.Testing;

namespace Lanceur.Tests.Utils
{
    internal static class MainViewModelHelper
    {
        #region Methods

        public static MainViewModel Build(TestScheduler scheduler, ISearchService searchService = null, ICmdlineManager cmdProcessor = null)
        {
            return new MainViewModel(
                scheduler,
                scheduler,
                ServiceFactory.LogService,
                searchService ?? ServiceFactory.SearchService,
                cmdProcessor ?? ServiceFactory.CmdLineService
            );
        }

        public static void SetResults(this MainViewModel vm, int count)
        {
            for (int i = 0; i < count; i++)
            {
                vm.Results.Add(NotExecutableTestAlias.FromName($"{i + 1}/{count}"));
            }
            vm.CurrentAlias = vm.Results.Count == 0 ? null : vm.Results[0];
        }

        #endregion Methods
    }
}