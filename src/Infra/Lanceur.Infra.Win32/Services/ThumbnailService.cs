using System.IO;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public class ThumbnailService : IThumbnailService
{
    #region Fields

    private readonly ILogger<ThumbnailService> _logger;
    private readonly IEnumerable<IThumbnailStrategy> _thumbnailStrategy;

    #endregion

    #region Constructors

    public ThumbnailService(
        ILoggerFactory loggerFactory,
        IEnumerable<IThumbnailStrategy> thumbnailStrategy
    )
    {
        _thumbnailStrategy = thumbnailStrategy;
        _logger = loggerFactory.GetLogger<ThumbnailService>();
    }

    #endregion

    #region Methods

    private static bool CanReturnEarly(QueryResult queryResult)
    {
        if (queryResult.IsThumbnailDisabled) return true;
        if (queryResult is not AliasQueryResult alias) return true;
        if (File.Exists(alias.Thumbnail)) return true;
        if (alias.FileName.IsNullOrEmpty()) return true;

        return false;
    }

    /// <summary>
    ///     Starts a thread to refresh the thumbnails asynchronously. This method returns immediately after starting the
    ///     thread.
    ///     Each time a thumbnail is found, the corresponding alias is updated. Because the alias is reactive, the UI will
    ///     automatically reflect these updates.
    /// </summary>
    /// <param name="queryResult">The list of queries for which thumbnails need to be updated.</param>
    public void UpdateThumbnail(QueryResult queryResult)
    {
        if (CanReturnEarly(queryResult)) return;
        
        _logger.LogTrace("Loading thumbnail for {AliasName}...", queryResult.Name);

        _ = RunStrategy(
            (AliasQueryResult)queryResult
        );

        return;
        
        async Task RunStrategy(AliasQueryResult alias)
        {
            foreach (var strategy in _thumbnailStrategy)
            {
                try { await strategy.UpdateThumbnailAsync(alias); }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "One thumbnail retrieve strategy failed: {Message}", ex.Message);
                }
            }
        }
    }

    #endregion
}