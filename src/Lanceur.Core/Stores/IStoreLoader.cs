using Lanceur.Core.Services;

namespace Lanceur.Core.Stores
{
    public interface IStoreLoader
    {
        #region Methods

        public IEnumerable<ISearchService> Load();

        #endregion Methods
    }
}