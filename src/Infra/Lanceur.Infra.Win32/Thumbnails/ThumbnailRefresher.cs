using System.IO;
using Lanceur.Core.Decorators;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Win32.Images;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails;

/// <summary>
/// Provides functionality to refresh the thumbnail based on the provided query.
/// This class handles executable files, Windows Store applications, and URLs by attempting
/// to retrieve their respective favicons when necessary.
/// </summary>
public class ThumbnailRefresher
{
    #region Fields

    private readonly IFavIconManager _favIconManager;
    private readonly ILogger<ThumbnailRefresher> _logger;
    private readonly IPackagedAppSearchService _searchService;
    private readonly ThumbnailLoader _thumbnailLoader;

    private const string WebIcon = "Web";

    #endregion

    #region Constructors

    public ThumbnailRefresher(ILoggerFactory loggerFactory, IPackagedAppSearchService searchService, IFavIconManager favIconManager)
    {
        _thumbnailLoader = new  (loggerFactory.CreateLogger<ThumbnailLoader>());
        _searchService = searchService;
        _favIconManager = favIconManager;
        _logger = loggerFactory.GetLogger<ThumbnailRefresher>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the thumbnail for the provided query. This method handles different types of sources:
    /// executables, Windows Store applications, and URLs. It attempts to retrieve and assign the appropriate
    /// thumbnail or favicon based on the query information.
    /// </summary>
    /// <param name="query">An object containing the necessary information to retrieve and update the thumbnail.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateThumbnailAsync(EntityDecorator<QueryResult> query)
    {
        if (query.Entity?.IsThumbnailDisabled ?? true) return;
        if (query.Entity is not AliasQueryResult alias) return;
        if (alias.FileName.IsNullOrEmpty()) return;

        if (File.Exists(alias.Thumbnail) || alias.Icon == WebIcon) return;

        var filePath = alias.FileName.GetThumbnailPath();
        if (File.Exists(filePath)) { alias.Thumbnail = filePath; }
        else if (alias.IsPackagedApplication())
        {
            var response = (await _searchService.GetByInstalledDirectory(alias.FileName))
                .FirstOrDefault();
            if (response is not null)
            {
                alias.Thumbnail = response.Logo.LocalPath;
                query.Soil();
            }

            alias.Thumbnail.CopyToImageRepository(alias.FileName);
            _logger.LogTrace("Retrieved thumbnail for packaged application {Name}. Thumbnail: {Thumbnail}", alias.Name, alias.Thumbnail);
            return;
        }

        var imageSource = _thumbnailLoader.GetThumbnail(alias.FileName);
        if (imageSource is not null)
        {
            var file = new FileInfo(alias.FileName);
            imageSource.CopyToImageRepository(file.Name);
            alias.Thumbnail = file.Name.GetThumbnailPath();
            query.Soil();
            _logger.LogTrace("Retrieved thumbnail for win32 application {Name}. Thumbnail: {Thumbnail}", alias.Name, alias.Thumbnail);
            return;
        }

        if (!alias.FileName.IsUrl()) return;

        var favicon = alias.FileName
                           .GetKeyForFavIcon()
                           .GetThumbnailPath();

        if (File.Exists(favicon))
        {
            alias.Thumbnail = favicon;
            query.Soil();
            return;
        }

        alias.Icon = WebIcon;
        alias.Thumbnail = null;

        _ = _favIconManager.RetrieveFaviconAsync(alias.FileName); // Fire & forget favicon retrieving
        _logger.LogTrace("Retrieved favicon for alias {Name}. Favicon {FileName}", alias.Name, alias.FileName);
    }

    #endregion
}