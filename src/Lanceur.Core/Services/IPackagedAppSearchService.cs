using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IPackagedAppSearchService
{
    #region Methods

    IEnumerable<PackagedApp> GetByInstalledDirectory(string fileName);

    #endregion Methods
}