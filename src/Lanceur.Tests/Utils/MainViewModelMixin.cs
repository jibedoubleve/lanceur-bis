using Lanceur.Tests.Utils.ReservedAliases;
using Lanceur.Views;

namespace Lanceur.Tests.Utils
{
    public static class MainViewModelMixin
    {
        #region Methods

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