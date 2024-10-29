using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows.Media;

namespace Lanceur.Infra.Win32.Thumbnails;

public class ThumbnailLoader
{
    #region Fields

    private readonly ILogger _logger;
    private static readonly Dictionary<string, ImageSource> Cache = new();

    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".ico" };

    private const int ThumbnailSize = 64;

    #endregion

    #region Constructors

    public ThumbnailLoader(ILogger<ThumbnailLoader> logger) => _logger = logger;

    #endregion

    #region Methods

    private ImageSource GetThumbnail(string path, ThumbnailOptions options) => WindowsThumbnailProvider.GetThumbnail(
        path,
        ThumbnailSize,
        ThumbnailSize,
        options
    );

    public ImageSource? GetThumbnail(string path)
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
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to extract thumbnail for {Path}", path); }

        //Return the value event if null;
        return image;
    }

    #endregion
}