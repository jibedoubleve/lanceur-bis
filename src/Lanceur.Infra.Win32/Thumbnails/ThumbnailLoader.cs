using System.IO;
using System.Windows.Media;
using Lanceur.Core.Services;
using Lanceur.Ui.Thumbnails;
using Splat;

namespace Lanceur.Infra.Win32.Thumbnails
{
    internal static class ThumbnailLoader
    {
        #region Fields

        private const int ThumbnailSize = 64;
        private static readonly IAppLoggerFactory AppLogFactory = Locator.Current.GetService<IAppLoggerFactory>()!;
        private static readonly Dictionary<string, ImageSource> Cache = new();

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
        public static ImageSource? GetThumbnail(string path)
        {
            ImageSource? image = null;
            try
            {
                if (string.IsNullOrEmpty(path)) { return null; }
                if (Cache.TryGetValue(path, out var value)) { return value; }
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
                    image = GetThumbnail(path, ImageExtensions.Contains(ext)
                                             /* Although the documentation for GetImage on MSDN indicates that
                                              * if a thumbnail is available it will return one, this has proved to not
                                              * be the case in many situations while testing.
                                              * - Solution: explicitly pass the ThumbnailOnly flag
                                              */
                                             ? ThumbnailOptions.ThumbnailOnly
                                             : ThumbnailOptions.None);
                }

                if (image != null)
                {
                    image.Freeze();
                    Cache[path] = image;
                }
            }
            catch (Exception ex)
            {
                AppLogFactory!.GetLogger(typeof(ThumbnailLoader)).Warning($"Failed to extract thumbnail for {path}", ex);
            }

            //Return the value event if null;
            return image;
        }

        #endregion Methods
    }
}