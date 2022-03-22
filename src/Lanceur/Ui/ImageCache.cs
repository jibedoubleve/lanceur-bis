using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;

namespace Lanceur.Ui
{
    public class ImageCache : IImageCache
    {
        #region Fields

        private readonly Dictionary<string, ImageSource> _cache = new();

        #endregion Fields

        #region Indexers

        public ImageSource this[string idx]
        {
            get => _cache[idx];
            set => _cache[idx] = value;
        }

        #endregion Indexers

        #region Methods

        public bool IsInCache(string path) => _cache.ContainsKey(path);

        public IEnumerator<KeyValuePair<string, ImageSource>> GetEnumerator() => _cache.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion Methods
    }
}