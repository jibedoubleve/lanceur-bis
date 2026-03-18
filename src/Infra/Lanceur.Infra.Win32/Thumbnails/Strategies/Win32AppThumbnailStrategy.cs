using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.Infra.Win32.Helpers;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public sealed class Win32AppThumbnailStrategy : ThumbnailStrategy
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
    ) : base(logger)
    {
        _staThreadRunner = staThreadRunner;
        _aliasManagementService = aliasManagementService;
        _logger = logger;
        _win32ThumbnailService = new Win32ThumbnailService(loggerFactory.CreateLogger<Win32ThumbnailService>());
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public override int Priority => 100;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override async Task<bool> UpdateThumbnailCoreAsync(AliasQueryResult alias, CancellationToken cancellationToken)
    {
        var imageSource = await _staThreadRunner.RunAsync(
            () => _win32ThumbnailService.GetThumbnail(alias.FileName!),
            cancellationToken
        );
        if (imageSource is null)
        {
            _logger.LogTrace("Failed to fetch the thumbnail in disk for alias {Name}.", alias.Name);
            return false;
        }

        alias.Thumbnail = alias.ResolveThumbnailAbsolutePath();
        imageSource.CopyToImageRepository(alias.Thumbnail);
        _aliasManagementService.UpdateThumbnail(alias);
        return true;
    }

    #endregion
}