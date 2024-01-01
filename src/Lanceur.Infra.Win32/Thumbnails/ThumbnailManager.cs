using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Lanceur.Core.Decorators;

namespace Lanceur.Infra.Win32.Thumbnails
{
    public class ThumbnailManager : IThumbnailManager
    {
        #region Fields

        private readonly IDbRepository _dbRepository;
        private readonly IAppLogger _log;
        private readonly IThumbnailRefresher _thumbnailRefresher;

        #endregion Fields

        #region Constructors

        public ThumbnailManager(
            IAppLoggerFactory loggerFactory,
            IDbRepository dbRepository,
            IThumbnailRefresher thumbnailRefresher)
        {
            _dbRepository = dbRepository;
            _thumbnailRefresher = thumbnailRefresher;
            _log = loggerFactory.GetLogger<ThumbnailManager>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Launch a thread to refresh the thumbnails and returns just after. Each time an thumbnail is found
        /// the alias is updated and (because the alias is reactive) the UI should be updated.
        /// </summary>
        /// <remarks>
        /// All the alias are updated at once to avoid concurrency issues.Thumbnail
        /// </remarks>
        /// <param name="results">The list a queries that need to have an updated thumbnail.</param>
        public async Task RefreshThumbnailsAsync(IEnumerable<QueryResult> results)
        {
            var queries = EntityDecorator<QueryResult>.FromEnumerable(results)
                                                      .ToArray();

            using var m = TimePiece.Measure(this, m => _log.Info(m));
            try
            {
                await Task.Run(() => Parallel.ForEach(queries, _thumbnailRefresher.RefreshCurrentThumbnail));

                var aliases = queries.Where(x => x.IsDirty)
                                     .Select(x => x.Entity)
                                     .OfType<AliasQueryResult>()
                                     .ToArray();
                if (aliases.Any())
                {
                    _dbRepository.UpdateThumbnails(aliases);
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"An error occured during the refresh of the icons. ('{ex.Message}')", ex);
            }
        }

        #endregion Methods
    }
}