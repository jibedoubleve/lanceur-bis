using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;
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

        public void RefreshThumbnails(IEnumerable<QueryResult> queries)
        {
            foreach (var query in queries)
            {
                if (query is AliasQueryResult alias)
                {
                    var path = alias.FileName;
                    if (path.IsNullOrEmpty()) { continue; }
                    else if (_cache.IsInCache(path))
                    {
                        alias.Thumbnail = _cache[path];
                    }
                    else
                    {
                        var task = Task.Run(() =>
                        {
                            var imgPath = File.Exists(query.Icon) ? query.Icon : path;
                            var src = ThumbnailLoader.Get(imgPath);
                            if (src is not null)
                            {
                                _cache[path] = src;
                            }
                            return src;
                        });
                        task.ContinueWith(t =>
                        {
                            alias.Thumbnail = t.Result;
                        });
                    }
                }
            }
        }

        #endregion Methods
    }
}