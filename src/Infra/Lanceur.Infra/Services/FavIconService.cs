using System.Text.RegularExpressions;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Services;

public sealed partial class FavIconService : IFavIconService
{
    #region Fields

    private readonly IFavIconDownloader _favIconDownloader;

    /// <summary>
    ///     A regex to check whether the specified text
    ///     is the template of a Macro
    /// </summary>
    private static readonly Regex IsMacroRegex = IsMacroRegexBuilder();

    #endregion

    #region Constructors

    public FavIconService(IFavIconDownloader favIconDownloader)
    {
        ArgumentNullException.ThrowIfNull(favIconDownloader);

        _favIconDownloader = favIconDownloader;
    }

    #endregion

    #region Methods

    [GeneratedRegex("@.*@")] private static partial Regex IsMacroRegexBuilder();

    /// <inheritdoc />
    public async Task<string> UpdateFaviconAsync(AliasQueryResult alias, Func<string, string> cachePathResolver)
    {
        var url = alias.FileName;
        // You don't need to check favicon when
        //   - no url is provided
        //   - alias is a Macro
        //   - Uri is malformed
        if (url is null || IsMacroRegex.Match(url).Success || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var favIconPath = cachePathResolver(uri.Host);

        if (File.Exists(favIconPath)) { return favIconPath; }

        var uriAuthority = uri.GetAuthority();
        var success = await _favIconDownloader.RetrieveAndSaveFavicon(uriAuthority, favIconPath);

        return success ? favIconPath : null;
    }

    #endregion
}