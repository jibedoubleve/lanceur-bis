using System.IO;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class PackagedAppThumbnailStrategy : IThumbnailStrategy
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
    )
    {
        _packagedAppSearchService = packagedAppSearchService;
        _aliasManagementService = aliasManagementService;
        _logger = logger;
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

        if (alias.FileName is null)
        {
            _logger.LogInformation("Alias {Alias} does not have a file name.", alias.FileName);
            return;
        }

        if (!alias.IsPackagedApplication()) { return; }

        var app = await _packagedAppSearchService.GetByInstalledDirectoryAsync(alias.FileName);
        var response = app.FirstOrDefault();

        if (response is null)
        {
            _logger.LogTrace("Failed to download the thumbnail for alias {Name}.", alias.Name);
            return;
        }

        var thumbnailFileName = alias.FileName.GetThumbnailFileName();
        response.Logo?.LocalPath.CopyToImageRepository(thumbnailFileName);
        alias.Thumbnail = thumbnailFileName.GetThumbnailAbsolutePath();
        _aliasManagementService.UpdateThumbnail(alias);
    }

    #endregion
}