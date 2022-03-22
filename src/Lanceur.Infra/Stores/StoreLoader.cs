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
            var stores = new List<ISearchService>();
            foreach (var type in found)
            {
                var instance = (ISearchService)Activator.CreateInstance(type);
                stores.Add(instance);
            }

            return stores;
        }

        #endregion Methods
    }
}