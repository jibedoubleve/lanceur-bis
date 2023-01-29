using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Stores;
using System.Reflection;

namespace Lanceur.Tests.Utils
{
    internal class DebugStoreLoader : IStoreLoader
    {
        #region Methods

        public IEnumerable<ISearchService> Load()
        {
            var results = new List<ISearchService>
            {
                new ReservedAliasStore(Assembly.GetExecutingAssembly(), ServiceFactory.DataService)
            };
            return results;
        }

        #endregion Methods
    }
}