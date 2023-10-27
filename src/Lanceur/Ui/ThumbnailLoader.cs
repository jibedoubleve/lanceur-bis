using Lanceur.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace Lanceur.Ui
{
    internal static class ThumbnailLoader
    {
        #region Fields

        private const int ThumbnailSize = 64;

        private static readonly Dictionary<string, ImageSource> _cache = new();

        private static readonly string[] ImageExtensions =
                {
            ".png",
            ".jpg",
            ".jpeg",
            ".gif",
            ".bmp",
            ".tiff",
            ".ico"
        };

        #endregion Fields

        #region Enums

        private enum ImageType
        {
            File,
            Folder,
            Data,
            ImageFile,
            Error,
            Cache
        }

        #endregion Enums

        #region Methods

        private static ImageSource GetThumbnail(string path, ThumbnailOptions options)
        {
            return WindowsThumbnailProvider.GetThumbnail(
                path,
                ThumbnailSize,
                ThumbnailSize,
                options
                );
        }

        public static ImageSource Get(string path)
        {
            ImageSource image = null;
            try
            {
                if (string.IsNullOrEmpty(path)) { return null; }
                if (_cache.ContainsKey(path)) { return _cache[path]; }

                if (Directory.Exists(path))
                {
                    /* Directories can also have thumbnails instead of shell icons.
                     * Generating thumbnails for a bunch of folders while scrolling through
                     * results from Everything makes a big impact on performance.
                     * - Solution: just load the icon
                     */
                    image = GetThumbnail(path, ThumbnailOptions.IconOnly);
                }
                else if (File.Exists(path))
                {
                    var ext = Path.GetExtension(path).ToLower();
                    if (ImageExtensions.Contains(ext))
                    {
                        /* Although the documentation for GetImage on MSDN indicates that
                         * if a thumbnail is available it will return one, this has proved to not
                         * be the case in many situations while testing.
                         * - Solution: explicitly pass the ThumbnailOnly flag
                         */
                        image = GetThumbnail(path, ThumbnailOptions.ThumbnailOnly);
                    }
                    else { image = GetThumbnail(path, ThumbnailOptions.None); }
                }

                if (image != null)
                {
                    image.Freeze();
                    _cache[path] = image;
                }
            }
            catch (Exception ex) { AppLogFactory.Get(typeof(ThumbnailLoader)).Warning($"Failed to extract thumbnail for {path}", ex); }

            //Return the value event if null;
            return image;
        }

        #endregion Methods
    }
}