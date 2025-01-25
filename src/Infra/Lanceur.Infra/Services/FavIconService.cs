using Lanceur.Core.Services;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Web;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Lanceur.SharedKernel.Mixins;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lanceur.Infra.Managers;

public class FavIconService : IFavIconService
{
    #region Fields

    /// <summary>
    /// A regex to check whether the specified text
    /// is the template of a Macro
    /// </summary>
    private static readonly Regex IsMacroRegex = new("@.*@");

    private readonly IFavIconDownloader _favIconDownloader;
    private readonly ILogger _logger;
    private readonly string _imageRepository;

    #endregion Fields

    #region Constructors

    /// <param name="imageRepository">
    /// This is used for unit tests. Keep default value unless you're testing
    /// </param>
    public FavIconService(
        IPackagedAppSearchService searchService,
        IFavIconDownloader favIconDownloader,
        ILoggerFactory loggerFactory,
        string imageRepository = null
    )
    {
        _imageRepository = imageRepository ?? Paths.ImageRepository;
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(favIconDownloader);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _favIconDownloader = favIconDownloader;
        _logger = loggerFactory.CreateLogger<FavIconDownloader>();
    }

    #endregion Constructors

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

    #endregion Methods
}