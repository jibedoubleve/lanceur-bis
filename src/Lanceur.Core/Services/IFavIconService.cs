using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IFavIconService
{
    #region Methods

    /// <summary>
    /// Retrieve favicon (if exists) and copy it into the thumbnails repository.
    /// If thumbnail exists, update the alias with the favicon information
    /// </summary>
    /// <param name="alias">The alias</param>
    Task RetrieveFaviconAsync(AliasQueryResult alias);

    #endregion Methods
}