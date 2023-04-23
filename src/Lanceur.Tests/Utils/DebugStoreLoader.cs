using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Stores;
using NSubstitute;
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
                new ReservedAliasStore(Assembly.GetExecutingAssembly(), Substitute.For<IDataService>())
            };
            return results;
        }

        #endregion Methods
    }
}