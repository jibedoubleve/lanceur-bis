using System.Collections.Generic;
using System.Windows.Media;

namespace Lanceur.Ui
{
    public interface IImageCache : IEnumerable<KeyValuePair<string, ImageSource>>
    {
        #region Indexers

        ImageSource this[string idx] { get; set; }

        #endregion Indexers

        #region Methods

        bool IsInCache(string path);

        /// <summary>
        /// Load the cache into memory. If implemented, it activate the mechanism to
        /// load from disk to memory.
        /// </summary>
        void LoadCache();

        #endregion Methods
    }
}