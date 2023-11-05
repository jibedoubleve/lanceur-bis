using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Images;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;

namespace Lanceur.Ui.Thumbnails
{
    public class WPFThumbnailManager : IThumbnailManager
    {
        #region Fields

        private readonly IImageCache _cache;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public WPFThumbnailManager(IImageCache cache, IAppLoggerFactory loggerFactory)
        {
            _cache = cache;
            _log = loggerFactory.GetLogger<WPFThumbnailManager>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Launch a thread to refresh the thumbnails and returns just after. Each time an thumbnail is found
        /// the alias is updated and (because the alias is reactive) the UI should be updated.
        /// </summary>
        /// <remarks>
        /// All the alias are updated at once to avoid concurrency issues.
        /// </remarks>
        /// <param name="queries">The list a queries that need to have an updated thumbnail.</param>
        public void RefreshThumbnails(IEnumerable<QueryResult> queries)
        {
            var t = Task.Run(() =>
            {
                foreach (var query in queries)
                {
                    if (query is not AliasQueryResult alias) continue;

                    var fileName = alias.FileName;

                    if (fileName.IsNullOrEmpty()) { continue; }

                    if (fileName.IsUrl() && _cache.IsInCache(fileName.GetKeyForFavIcon()))
                    {
                        alias.Thumbnail = _cache[fileName.GetKeyForFavIcon()];
                        continue;
                    }
                    if (_cache.IsInCache(fileName))
                    {
                        alias.Thumbnail = _cache[fileName];
                        continue;
                    }
                    
                    if (alias.IsPackagedApplication() && alias.Thumbnail is string)
                    {
                         ((string)alias.Thumbnail).CopyToCache(alias.FileName);
                        continue;
                    }
                    
                    var imageSource = ThumbnailLoader.GetThumbnail(fileName);
                    var file = new FileInfo(fileName);
                    imageSource.CopyToCache($"{file.Name}.png");
                    _cache[fileName] = imageSource;
                    alias.Thumbnail = imageSource;
                }
            });
            t.ContinueWith(t =>
            {
                _log.Warning($"An error occured during the refresh of the icons. ('{t.Exception.Message}')", t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion Methods
    }
}