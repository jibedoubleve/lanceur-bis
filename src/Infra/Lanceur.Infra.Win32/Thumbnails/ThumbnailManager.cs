using Lanceur.Core.Decorators;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails;

public class ThumbnailManager : IThumbnailManager
{
    #region Fields

    private readonly IDbRepository _dbRepository;
    private readonly ILogger<ThumbnailManager> _logger;
    private readonly IThumbnailRefresher _thumbnailRefresher;

    #endregion Fields

    #region Constructors

    public ThumbnailManager(
        ILoggerFactory loggerFactory,
        IDbRepository dbRepository,
        IThumbnailRefresher thumbnailRefresher
    )
    {
        _dbRepository = dbRepository;
        _thumbnailRefresher = thumbnailRefresher;
        _logger = loggerFactory.GetLogger<ThumbnailManager>();
    }

    #endregion Constructors

    #region Methods

    /// <summary>
    /// Starts a thread to refresh the thumbnails asynchronously. This method returns immediately after starting the thread.
    /// Each time a thumbnail is found, the corresponding alias is updated. Because the alias is reactive, the UI will
    /// automatically reflect these updates.
    /// </summary>
    /// <remarks>
    /// All aliases are updated in a single operation to avoid concurrency issues.
    /// </remarks>
    /// <param name="results">The list of queries for which thumbnails need to be updated.</param>
    public void RefreshThumbnails(IEnumerable<QueryResult> results)
    {
        var queries = EntityDecorator<QueryResult>.FromEnumerable(results)
                                                  .ToArray();

        using var m = _logger.MeasureExecutionTime(this);
        try
        {
            var tasks = queries.Select(q => _thumbnailRefresher.RefreshCurrentThumbnailAsync(q));
            _ =  Task.WhenAll(tasks); // Fire & forget thumbnail refresh

            var aliases = queries.Where(x => x.IsDirty)
                                 .Select(x => x.Entity)
                                 .OfType<AliasQueryResult>()
                                 .ToArray();
            if (aliases.Any()) _dbRepository.UpdateThumbnails(aliases);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "An error occured during the refresh of the icons"); }
    }

    #endregion Methods
}