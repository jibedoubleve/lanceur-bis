using Lanceur.Core.Decorators;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails;

public class ThumbnailService : IThumbnailService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    private readonly ILogger<ThumbnailService> _logger;
    private readonly ThumbnailRefresher _thumbnailRefresher;

    #endregion

    #region Constructors

    public ThumbnailService(
        ILoggerFactory loggerFactory,
        IAliasRepository aliasRepository,
        IPackagedAppSearchService packagedAppSearchService,
        IFavIconService favIconService
    )
    {
        _aliasRepository = aliasRepository;
        _thumbnailRefresher = new(loggerFactory, packagedAppSearchService, favIconService);
        _logger = loggerFactory.GetLogger<ThumbnailService>();
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Starts a thread to refresh the thumbnails asynchronously. This method returns immediately after starting the
    ///     thread.
    ///     Each time a thumbnail is found, the corresponding alias is updated. Because the alias is reactive, the UI will
    ///     automatically reflect these updates.
    /// </summary>
    /// <remarks>
    ///     All aliases are updated in a single operation to avoid concurrency issues.
    /// </remarks>
    /// <param name="results">The list of queries for which thumbnails need to be updated.</param>
    public void RefreshThumbnails(IEnumerable<QueryResult> results)
    {
        var queries = EntityDecorator<QueryResult>.FromEnumerable(results)
                                                  .ToArray();

        using var m = _logger.MeasureExecutionTime(this);
        try
        {
            var tasks = queries.Select(_thumbnailRefresher.UpdateThumbnailAsync);
            _ =  Task.WhenAll(tasks); // Fire & forget thumbnail refresh

            var aliases = queries.Where(x => x.IsDirty)
                                 .Select(x => x.Entity)
                                 .OfType<AliasQueryResult>()
                                 .ToArray();
            if (aliases.Any()) _aliasRepository.UpdateThumbnails(aliases);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "An error occured during the refresh of the icons"); }
    }

    #endregion
}