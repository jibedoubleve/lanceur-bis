namespace Lanceur.Core.Services;

/// <summary>
///     Downloads the favicon of a website from the specified URL and saves it to disk.
/// </summary>
public interface IFavIconDownloader
{
    #region Methods

    /// <summary>
    ///     Downloads the favicon from the given URL and saves it to the specified output path.
    ///     If no favicon is found, nothing is saved.
    /// </summary>
    /// <param name="url">The URL of the website whose favicon should be retrieved.</param>
    /// <param name="outputPath">The file path where the favicon will be saved.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if a favicon was found and saved; otherwise <c>false</c>.</returns>
    Task<bool> RetrieveAndSaveFavicon(Uri url, string outputPath, CancellationToken cancellationToken);

    #endregion
}