using System.Text.RegularExpressions;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Web;

namespace Lanceur.Infra.Services;

public class FavIconService : IFavIconService
{
    #region Fields

    private readonly IFavIconDownloader _favIconDownloader;
    private readonly string _imageRepository;

    /// <summary>
    ///     A regex to check whether the specified text
    ///     is the template of a Macro
    /// </summary>
    private static readonly Regex IsMacroRegex = new("@.*@");

    #endregion

    #region Constructors

    public FavIconService(
        IPackagedAppSearchService searchService,
        IFavIconDownloader favIconDownloader,
        string imageRepository = null
    )
    {
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(favIconDownloader);

        _imageRepository = imageRepository ?? Paths.ImageRepository;
        _favIconDownloader = favIconDownloader;
    }

    #endregion

    #region Methods

    public async Task RetrieveFaviconAsync(AliasQueryResult alias)
    {
        var url = alias.FileName;
        if (url is null) return;
        if (IsMacroRegex.Match(url).Success) return;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;

        var favIconPath = Path.Combine(_imageRepository, $"{FavIconExtensions.FilePrefix}{uri.Host}.ico");
        if (File.Exists(favIconPath))
        {
            alias.Thumbnail = favIconPath;
            return;
        }

        var uriAuthority = uri.GetAuthority();
        var success = await _favIconDownloader.SaveToFileAsync(uriAuthority, favIconPath);
        
        alias.Thumbnail = success ? favIconPath : null;
    }

    #endregion
}