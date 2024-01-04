using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IPackagedAppSearchService
{
    #region Methods

    Task<IEnumerable<PackagedApp>> GetByInstalledDirectory(string fileName);

    #endregion Methods
}