using System.IO;
using Lanceur.Core.Decorators;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Images;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Win32.Thumbnails;

public class ThumbnailRefresher : IThumbnailRefresher
{
    #region Fields

    private const string WebIcon = "Web";
    private readonly IFavIconManager _favIconManager;
    private readonly IAppLogger _log;
    private readonly IPackagedAppSearchService _searchService;

    #endregion Fields

    #region Constructors

    public ThumbnailRefresher(IAppLoggerFactory loggerFactory, IPackagedAppSearchService searchService, IFavIconManager favIconManager)
    {
        _searchService  = searchService;
        _favIconManager = favIconManager;
        _log            = loggerFactory.GetLogger<ThumbnailRefresher>();
    }

    #endregion Constructors

    #region Methods

    public void RefreshCurrentThumbnail(EntityDecorator<QueryResult> query)
    {
        if (query.Entity is not AliasQueryResult alias) return;
        if (alias.FileName.IsNullOrEmpty()) return;
        if (File.Exists(alias.Thumbnail) || alias.Icon == WebIcon)
        {
            _log.Trace($"A thumbnail already exists for '{alias.Name}'. Thumbnail: '{alias.Thumbnail ?? WebIcon}'");
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
            _log.Trace($"Retrieved thumbnail for packaged app '{alias.Name}'. Thumbnail: '{alias.Thumbnail}'");
            return;
        }

        var imageSource = ThumbnailLoader.GetThumbnail(alias.FileName);
        if (imageSource is not null)
        {
            var file = new FileInfo(alias.FileName);
            imageSource.CopyToImageRepository(file.Name);
            alias.Thumbnail = file.Name.ToAbsolutePath();
            query.Soil();
            _log.Trace($"Retrieved thumbnail for win32 application'{alias.Name}'. Thumbnail: '{alias.Thumbnail}'");
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

        alias.Icon      = WebIcon;
        alias.Thumbnail = favicon;

        _ = _favIconManager.RetrieveFaviconAsync(alias.FileName); // Fire & forget favicon retrieving
        _log.Trace($"Retrieved favicon for alias '{alias.Name}'. Favicon '{alias.FileName}'");
    }

    #endregion Methods
}