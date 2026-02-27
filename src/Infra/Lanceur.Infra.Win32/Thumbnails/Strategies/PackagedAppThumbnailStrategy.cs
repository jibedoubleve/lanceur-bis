using System.IO;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class PackagedAppThumbnailStrategy : IThumbnailStrategy
{
    #region Fields

    private readonly IPackagedAppSearchService _packagedAppSearchService;
    private readonly IAliasManagementService _aliasManagementService;

    #endregion

    #region Constructors

    public PackagedAppThumbnailStrategy(
        IPackagedAppSearchService packagedAppSearchService,
        IAliasManagementService aliasManagementService
    )
    {
        _packagedAppSearchService = packagedAppSearchService;
        _aliasManagementService = aliasManagementService;
    }

    #endregion

    #region Methods

    public async Task UpdateThumbnailAsync(AliasQueryResult alias)
    {
        if (File.Exists(alias.Thumbnail)) return;
        if (!alias.IsPackagedApplication()) return;

        var app = await _packagedAppSearchService.GetByInstalledDirectoryAsync(alias.FileName);
        var response = app.FirstOrDefault();

        if (response is null) return;

        var thumbnailFileName = alias.FileName.GetThumbnailFileName();
        response.Logo.LocalPath.CopyToImageRepository(thumbnailFileName);
        alias.Thumbnail = thumbnailFileName.GetThumbnailAbsolutePath();
        _aliasManagementService.UpdateThumbnail(alias);
    }

    #endregion
}