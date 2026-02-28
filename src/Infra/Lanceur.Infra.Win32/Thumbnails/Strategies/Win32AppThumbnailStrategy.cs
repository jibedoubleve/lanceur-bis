using System.IO;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.Infra.Win32.Helpers;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class Win32AppThumbnailStrategy : IThumbnailStrategy
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;

    private readonly IStaThreadRunner _staThreadRunner;
    private readonly Win32ThumbnailService _win32ThumbnailService;

    #endregion

    #region Constructors

    public Win32AppThumbnailStrategy(
        IStaThreadRunner staThreadRunner,
        ILoggerFactory loggerFactory,
        IAliasManagementService  aliasManagementService
    )
    {
        _staThreadRunner = staThreadRunner;
        _aliasManagementService = aliasManagementService;
        _win32ThumbnailService = new(loggerFactory.CreateLogger<Win32ThumbnailService>());
    }

    #endregion

    #region Methods

    public async Task UpdateThumbnailAsync(AliasQueryResult alias)
    {
        if (File.Exists(alias.Thumbnail)) { return; }

        var imageSource = await _staThreadRunner.RunAsync(() => _win32ThumbnailService.GetThumbnail(alias.FileName));
        if (imageSource is null) { return; }

        var thumbnailFileName = alias.FileName.GetThumbnailFileName();
        imageSource.CopyToImageRepository(thumbnailFileName);
        alias.Thumbnail = thumbnailFileName.GetThumbnailAbsolutePath();
        _aliasManagementService.UpdateThumbnail(alias);
    }

    #endregion
}