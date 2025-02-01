using System.Text.RegularExpressions;
using Lanceur.Core.Services;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Web;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lanceur.Infra.Managers;

public class FavIconService : IFavIconService
{
    #region Fields

    private readonly IFavIconDownloader _favIconDownloader;
    private readonly string _imageRepository;
    private readonly ILogger _logger;

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
        ILoggerFactory loggerFactory,
        string imageRepository = null
    )
    {
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(favIconDownloader);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _imageRepository = imageRepository ?? Paths.ImageRepository;
        _favIconDownloader = favIconDownloader;
        _logger = loggerFactory.CreateLogger<FavIconDownloader>();
    }

    #endregion

    #region Methods

    public async Task RetrieveFaviconAsync(string url)
    {
        if (url is null) return;
        if (IsMacroRegex.Match(url).Success) return;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;

        var output = Path.Combine(_imageRepository, $"{FavIconHelpers.FilePrefix}{uri.Host}.png");
        if (File.Exists(output)) return;

        var uriAuthority = uri.GetAuthority();

        if (!await _favIconDownloader.CheckExistsAsync(uriAuthority)) return;

        await _favIconDownloader.SaveToFileAsync(uriAuthority, output);
    }

    #endregion
}