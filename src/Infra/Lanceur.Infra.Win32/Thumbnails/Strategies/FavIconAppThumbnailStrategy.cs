using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class FavIconAppThumbnailStrategy : ThumbnailStrategy
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
    ) : base(logger)
    {
        _favIconService = favIconService;
        _logger = logger;
        _aliasManagementService = aliasManagementService;
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override async Task<bool> UpdateThumbnailCoreAsync(AliasQueryResult alias, CancellationToken cancellationToken)
    {
        var thumbnail = await _favIconService.UpdateFaviconAsync(
            alias,
            _ => alias.ResolveThumbnailAbsolutePath(),
            cancellationToken
        );
        if (thumbnail.IsNullOrEmpty()) { return false; }

        _logger.LogInformation(
            "Updating for alias {Alias} favicon from {Thumbnail} to {FavIconPath}",
            alias.Name.DefaultIfNullOrEmpty("<empty>"),
            alias.Thumbnail?.DefaultIfNullOrEmpty("<empty>"),
            thumbnail.DefaultIfNullOrEmpty("<empty>")
        );

        alias.Thumbnail = thumbnail;
        _aliasManagementService.UpdateThumbnail(alias);
        return true;
    }

    #endregion
}