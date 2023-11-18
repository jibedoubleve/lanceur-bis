using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Images;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;
using System.IO;
using Lanceur.Core.Decorators;

namespace Lanceur.Infra.Win32.Thumbnails
{
    public class ThumbnailManager : IThumbnailManager
    {
        #region Fields

        private readonly IDbRepository _dbRepository;
        private readonly IPackagedAppSearchService _searchService;
        private readonly IAppLogger _log;
        private readonly IThumbnailFixer _thumbnailFixer;

        #endregion Fields

        #region Constructors

        public ThumbnailManager(
            IAppLoggerFactory loggerFactory, 
            IThumbnailFixer thumbnailFixer, 
            IDbRepository dbRepository,
            IPackagedAppSearchService searchService)
        {
            _thumbnailFixer = thumbnailFixer;
            _dbRepository = dbRepository;
            _searchService = searchService;
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
        public void RefreshThumbnails(IEnumerable<QueryResult> results)
        {
            var queries = EntityDecorator<QueryResult>.FromEnumerable(results)
                                                      .ToArray();
            
            using var m = TimePiece.Measure(this, m => _log.Info(m));
            const string webIcon = "Web";
            try
            {
                Parallel.ForEach(queries, RefreshCurrentThumbnail);

                var aliases = queries.Where(x=>x.IsDirty)
                                     .Select(x => x.Entity)
                                     .OfType<AliasQueryResult>()
                                     .ToArray();
                if (aliases.Any())
                {
                    _dbRepository.UpdateMany(aliases);
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"An error occured during the refresh of the icons. ('{ex.Message}')", ex);
            }

            return;

            void RefreshCurrentThumbnail(EntityDecorator<QueryResult> query)
            {
                if (query.Entity is not AliasQueryResult alias) return;
                if (alias.FileName.IsNullOrEmpty()) return;
                if (File.Exists(alias.Thumbnail) || alias.Icon == webIcon)
                {
                    _log.Trace($"A thumbnail already exists for '{alias.Name}'. Thumbnail: '{alias.Thumbnail ?? webIcon}'");
                    return;
                }
                if (alias.IsPackagedApplication())
                {
                    var response = _searchService.GetByInstalledDirectory(alias.FileName)
                                                 .FirstOrDefault();
                    if (response is not null)
                    {
                        alias.Thumbnail = response.Logo.LocalPath;
                        query.Soil();
                    }

                    alias.Thumbnail.CopyToImageRepository(alias.FileName);
                    _log.Trace($"Retrieved thumbnail for packaged app '{alias.Name}'. Thumbnail: '{alias.Thumbnail}'");
                    return;
                }

                var imageSource = ThumbnailLoader.GetThumbnail(alias.FileName);
                if (imageSource is not null)
                {
                    var file = new FileInfo(alias.FileName);
                    imageSource.CopyToImageRepository(file.Name);
                    alias.Thumbnail = file.Name.ToAbsolutePath();
                    query.Soil();
                    _log.Trace($"Retrieved thumbnail for win32 application'{alias.Name}'. Thumbnail: '{alias.Thumbnail}'");
                    return;
                }

                if (!alias.FileName.IsUrl()) return;

                var favicon = alias.FileName
                                   .GetKeyForFavIcon()
                                   .ToAbsolutePath();

                if (File.Exists(favicon))
                {
                    alias.Thumbnail = favicon;
                    query.Soil();
                    return;
                }

                alias.Icon = webIcon;
                alias.Thumbnail = favicon;

                _ = _thumbnailFixer.RetrieveFaviconAsync(alias.FileName); // Fire & forget favicon retrieving
                _log.Trace($"Retrieved favicon for alias '{alias.Name}'. Favicon '{alias.FileName}'");
            }
        }

        #endregion Methods
    }
}