using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;
using System.IO;
using System.Windows.Media;

namespace Lanceur.Infra.Win32.Thumbnails;

internal static class ThumbnailLoader
{
    #region Fields

    private const int ThumbnailSize = 64;
    private static readonly Microsoft.Extensions.Logging.ILogger Logger;
    private static readonly Dictionary<string, ImageSource> Cache = new();

    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".ico" };

    #endregion Fields

    #region Constructors

    static ThumbnailLoader()
    {
        var factory = Locator.Current.GetService<ILoggerFactory>();
        Logger = factory.GetLogger(typeof(ThumbnailLoader));
    }

    #endregion Constructors

    #region Methods

    private static ImageSource GetThumbnail(string path, ThumbnailOptions options) => WindowsThumbnailProvider.GetThumbnail(
        path,
        ThumbnailSize,
        ThumbnailSize,
        options
    );

    public static ImageSource? GetThumbnail(string path)
    {
        ImageSource? image = null;
        try
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (Cache.TryGetValue(path, out var value)) return value;

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
                image = GetThumbnail(
                    path,
                    ImageExtensions.Contains(ext)
                        /* Although the documentation for GetImage on MSDN indicates that
                         * if a thumbnail is available it will return one, this has proved to not
                         * be the case in many situations while testing.
                         * - Solution: explicitly pass the ThumbnailOnly flag
                         */
                        ? ThumbnailOptions.ThumbnailOnly
                        : ThumbnailOptions.None
                );
            }

            if (image != null)
            {
                image.Freeze();
                Cache[path] = image;
            }
        }
        catch (Exception ex) { Logger.LogWarning(ex, "Failed to extract thumbnail for {Path}", path); }

        //Return the value event if null;
        return image;
    }

    #endregion Methods
}