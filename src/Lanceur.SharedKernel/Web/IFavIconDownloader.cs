namespace Lanceur.SharedKernel.Web;

/// <summary>
///     This class will download the favicon of the the website defined by the
///     specified URL
/// </summary>
public interface IFavIconDownloader
{
    #region Methods

    /// <summary>
    ///     Download the favicon and save it to a file. If there's no
    ///     favicon, nothing is saved
    /// </summary>
    /// <param name="url">The url of the website</param>
    /// <param name="outputPath">The path of the file to create.</param>
    /// <returns><c>True</c> if favicon was found at the specified address; otherwise <c>False</c></returns>
    Task<bool> RetrieveAndSaveFavicon(Uri url, string outputPath);

    #endregion
}