using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IFavIconService
{
    #region Methods

    /// <summary>
    ///     Attempts to download the favicon of the website whose URL is defined in
    ///     <see cref="AliasQueryResult.FileName" /> and saves it to a local disk cache.
    ///     If <c>FileName</c> is not a valid URL, or if the download fails for any reason,
    ///     the method returns <c>null</c> instead of throwing.
    /// </summary>
    /// <param name="alias">
    ///     The alias whose <c>FileName</c> is the URL of the website to retrieve the favicon for.
    ///     If <c>FileName</c> is not a URL, no download is attempted.
    /// </param>
    /// <param name="cachePathResolver">
    ///     A function that computes the absolute path on disk where the favicon will be cached,
    ///     given a raw file name (e.g. a hashed name).
    /// </param>
    /// <returns>
    ///     The absolute path to the cached favicon file if the download succeeded;
    ///     <c>null</c> if <c>FileName</c> is not a URL, the favicon could not be found, or the download failed.
    /// </returns>
    Task<string> UpdateFaviconAsync(AliasQueryResult alias, Func<string, string> cachePathResolver);

    #endregion
}