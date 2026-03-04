using System.IO;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class FavIconAppThumbnailStrategy : IThumbnailStrategy
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;
    private readonly IFavIconService _favIconService;
    private readonly ILogger<FavIconAppThumbnailStrategy> _logger;

    #endregion

    #region Constructors

    public FavIconAppThumbnailStrategy(
        IFavIconService favIconService,
        ILogger<FavIconAppThumbnailStrategy> logger,
        IAliasManagementService aliasManagementService
    )
    {
        _favIconService = favIconService;
        _logger = logger;
        _aliasManagementService = aliasManagementService;
    }

    #endregion

    #region Methods

    public async Task UpdateThumbnailAsync(AliasQueryResult alias)
    {
        if (File.Exists(alias.Thumbnail))
        {
            _logger.LogTrace("Thumbnail for alias {Name} is in cache. Update skipped.", alias.Name);
            return;
        }

        var thumbnail = await _favIconService.UpdateFaviconAsync(alias, ResolveCachePath);
        if (thumbnail.IsNullOrEmpty()) { return; }

        _logger.LogInformation(
            "Updating for alias {Alias} favicon from {Thumbnail} to {FavIconPath}",
            alias.Name.DefaultIfNullOrEmpty("<empty>"),
            alias.Thumbnail.DefaultIfNullOrEmpty("<empty>"),
            thumbnail.DefaultIfNullOrEmpty("<empty>")
        );

        alias.Thumbnail = thumbnail;
        _aliasManagementService.UpdateThumbnail(alias);

        return;

        string ResolveCachePath(string p) => p.GetThumbnailFileName().GetThumbnailAbsolutePath();
    }

    #endregion
}