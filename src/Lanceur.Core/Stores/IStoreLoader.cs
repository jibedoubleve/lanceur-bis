using Lanceur.Core.Services;

namespace Lanceur.Core.Stores;

public interface IStoreLoader
{
    #region Methods

    public IEnumerable<IStoreService> Load();

    #endregion Methods
}