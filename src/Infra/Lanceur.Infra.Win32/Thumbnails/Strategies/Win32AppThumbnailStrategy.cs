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
    private readonly ILogger<Win32AppThumbnailStrategy> _logger;

    private readonly IStaThreadRunner _staThreadRunner;
    private readonly Win32ThumbnailService _win32ThumbnailService;

    #endregion

    #region Constructors

    public Win32AppThumbnailStrategy(
        IStaThreadRunner staThreadRunner,
        ILoggerFactory loggerFactory,
        IAliasManagementService aliasManagementService,
        ILogger<Win32AppThumbnailStrategy> logger
    )
    {
        _staThreadRunner = staThreadRunner;
        _aliasManagementService = aliasManagementService;
        _logger = logger;
        _win32ThumbnailService = new Win32ThumbnailService(loggerFactory.CreateLogger<Win32ThumbnailService>());
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

        var imageSource = await _staThreadRunner.RunAsync(() => _win32ThumbnailService.GetThumbnail(alias.FileName));
        if (imageSource is null)
        {
            _logger.LogTrace("Failed to download the thumbnail for alias {Name}.", alias.Name);
            return;
        }

        var thumbnailFileName = alias.FileName?.GetThumbnailFileName();
        imageSource.CopyToImageRepository(thumbnailFileName);
        alias.Thumbnail = thumbnailFileName?.GetThumbnailAbsolutePath();
        _aliasManagementService.UpdateThumbnail(alias);
    }

    #endregion
}