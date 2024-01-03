using Lanceur.Core.Decorators;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Win32.Images;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Lanceur.Infra.Win32.Thumbnails;

public class ThumbnailRefresher : IThumbnailRefresher
{
    #region Fields

    private const string WebIcon = "Web";
    private readonly IFavIconManager _favIconManager;
    private readonly ILogger<ThumbnailRefresher> _logger;
    private readonly IPackagedAppSearchService _searchService;

    #endregion Fields

    #region Constructors

    public ThumbnailRefresher(ILoggerFactory loggerFactory, IPackagedAppSearchService searchService, IFavIconManager favIconManager)
    {
        _searchService = searchService;
        _favIconManager = favIconManager;
        _logger = loggerFactory.GetLogger<ThumbnailRefresher>();
    }

    #endregion Constructors

    #region Methods

    public void RefreshCurrentThumbnail(EntityDecorator<QueryResult> query)
    {
        if (query.Entity is not AliasQueryResult alias) return;
        if (alias.FileName.IsNullOrEmpty()) return;
        if (File.Exists(alias.Thumbnail) || alias.Icon == WebIcon)
        {
            var thumbnail = alias.Thumbnail ?? WebIcon;
            _logger.LogTrace("A thumbnail already exists for {Name}. Thumbnail: {Thumbnail}", alias.Name, thumbnail);
            return;
        }
        if (alias.IsPackagedApplication())
        {
            var response = _searchService.GetByInstalledDirectory(alias.FileName)
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

        var imageSource = ThumbnailLoader.GetThumbnail(alias.FileName);
        if (imageSource is not null)
        {
            var file = new FileInfo(alias.FileName);
            imageSource.CopyToImageRepository(file.Name);
            alias.Thumbnail = file.Name.ToAbsolutePath();
            query.Soil();
            _logger.LogTrace("Retrieved thumbnail for win32 application {Name}. Thumbnail: {Thumbnail}", alias.Name, alias.Thumbnail);
            return;
        }

        if (!alias.FileName.IsUrl()) return;

        var favicon = alias.FileName
                           .GetKeyForFavIcon()
                           .ToAbsolutePath();

        if (File.Exists(favicon))
        {
            alias.Thumbnail = favicon;
            query.Soil();
            return;
        }

        alias.Icon = WebIcon;
        alias.Thumbnail = favicon;

        _ = _favIconManager.RetrieveFaviconAsync(alias.FileName); // Fire & forget favicon retrieving
        _logger.LogTrace("Retrieved favicon for alias {Name}. Favicon {FileName}", alias.Name, alias.FileName);
    }

    #endregion Methods
}