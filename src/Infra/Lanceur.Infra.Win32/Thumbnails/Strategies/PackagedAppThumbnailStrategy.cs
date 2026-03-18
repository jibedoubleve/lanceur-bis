using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class PackagedAppThumbnailStrategy : ThumbnailStrategy
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;
    private readonly ILogger<PackagedAppThumbnailStrategy> _logger;

    private readonly IPackagedAppSearchService _packagedAppSearchService;

    #endregion

    #region Constructors

    public PackagedAppThumbnailStrategy(
        IPackagedAppSearchService packagedAppSearchService,
        IAliasManagementService aliasManagementService,
        ILogger<PackagedAppThumbnailStrategy> logger
    ) : base(logger)
    {
        _packagedAppSearchService = packagedAppSearchService;
        _aliasManagementService = aliasManagementService;
        _logger = logger;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public override int Priority => 200;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override async Task<bool> UpdateThumbnailCoreAsync(AliasQueryResult alias, CancellationToken cancellationToken)
    {
        if (!alias.IsPackagedApplication()) { return false; }

        var app = await _packagedAppSearchService.GetByInstalledDirectoryAsync(alias.FileName!);
        var response = app.FirstOrDefault();

        if (response is null)
        {
            _logger.LogTrace("Failed to download the thumbnail for alias {Name}.", alias.Name);
            return true;
        }

        var thumbnailAbsolutePath = alias.ResolveThumbnailAbsolutePath();
        response.Logo.LocalPath.CopyToImageRepository(thumbnailAbsolutePath);
        alias.Thumbnail = thumbnailAbsolutePath;
        _aliasManagementService.UpdateThumbnail(alias);
        return true;
    }

    #endregion
}