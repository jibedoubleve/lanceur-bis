using Lanceur.Core.Models;
using Lanceur.Tests.Utils.ReservedAliases;
using Lanceur.Views;

namespace Lanceur.Tests.Utils
{
    public static class MainViewModelTestHelper
    {
        #region Methods

        public static IEnumerable<QueryResult> BuildResults(int count)
        {
            var result = new List<QueryResult>();
            for (int i = 0; i < count; i++)
            {
                result.Add(NotExecutableTestAlias.FromName($"{i + 1}/{count}"));
            }
            return result;
        }

        public static void SetResults(this MainViewModel vm, IEnumerable<QueryResult> results)
        {
            foreach (var item in results)
            {
                vm.Results.Add(item);
            }
        }

        #endregion Methods
    }
}