using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Stores;

namespace Lanceur.Tests.Utils
{
    internal class DebugStoreLoader : IStoreLoader
    {
        #region Methods

        public IEnumerable<ISearchService> Load()
        {
            var results = new List<ISearchService>
            {
                new ReservedAliasStore()
            };
            return results;
        }

        #endregion Methods
    }
}