namespace Lanceur.Core.Services;

public interface IFavIconService
{
    #region Methods

    /// <summary>
    /// Retrieve favicon (if exists) and copy it into the thumbnails repository
    /// </summary>
    /// <param name="url">The url configured in the alias</param>
    Task RetrieveFaviconAsync(string url);

    #endregion Methods
}