using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lanceur.Ui
{
    public class WPFThumbnailManager : IThumbnailManager
    {
        #region Fields

        private readonly IImageCache _cache;

        #endregion Fields

        #region Constructors

        public WPFThumbnailManager(IImageCache cache)
        {
            _cache = cache;
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// Launch a thread to refresh the thumbnails and returns just after. Each time an thumbnail is found
        /// the alias is updated and (because the alias is reactive) the UI should be updated.
        /// </summary>
        /// <remarks>
        /// All the alias are updated at once to avoid concurency issues.
        /// </remarks>
        /// <param name="queries">The list a queries that need to have an updated thumbnail.</param>
        public void RefreshThumbnails(IEnumerable<QueryResult> queries)
        {
            var t = Task.Run(() =>
            {
                foreach (var query in queries)
                {
                    if (query is AliasQueryResult alias)
                    {
                        var path = alias.FileName;
                        AppLogFactory.Get<WPFThumbnailManager>().Trace($"Refresh thumbnail of '{path}'.");

                        if (path.IsNullOrEmpty()) { continue; }
                        else if (_cache.IsInCache(path))
                        {
                            alias.Thumbnail = _cache[path];
                        }
                        else
                        {
                            var imgPath = File.Exists(query.Icon) ? query.Icon : path;
                            var src = ThumbnailLoader.Get(imgPath);
                            if (src is not null)
                            {
                                _cache[path] = src;
                            }
                            alias.Thumbnail = src;
                        }
                    }
                }
            });
            t.ContinueWith(t =>
            {
                AppLogFactory.Get<WPFThumbnailManager>().Warning($"An error occured during the refresh of the icons. ('{t.Exception.Message}')", t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion Methods
    }
}