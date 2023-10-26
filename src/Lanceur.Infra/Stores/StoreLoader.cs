using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using System.Reflection;

namespace Lanceur.Infra.Stores
{
    public class StoreLoader : IStoreLoader
    {
        #region Methods

        public IEnumerable<ISearchService> Load()
        {
            var asm = Assembly.GetAssembly(typeof(SearchService));
            var types = asm.GetTypes();

            var found = (from t in types
                         where t.GetCustomAttributes(typeof(StoreAttribute)).Any()
                         select t).ToList();

            return found.Select(type => (ISearchService)Activator.CreateInstance(type))
                        .ToList();
        }

        #endregion Methods
    }
}