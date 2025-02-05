using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails;

/// <summary>
/// This class is responsible for loading the thumbnail of a Win32 application (not a packaged app) or a directory.
/// It provides functionality to retrieve and display preview images associated with executable files or folder contents.
/// </summary>
internal class Win32ThumbnailService

{
    #region Fields

    private readonly ILogger _logger;
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".ico" };
    private const int ThumbnailSize = 64;

    #endregion

    #region Constructors

    public Win32ThumbnailService(ILogger<Win32ThumbnailService> logger) => _logger = logger;

    #endregion

    #region Methods

    private static BitmapSource GetThumbnail(string path, ThumbnailOptions options) => Win32ThumbnailProvider.GetThumbnail(
        path,
        ThumbnailSize,
        ThumbnailSize,
        options
    );

    public ImageSource? GetThumbnail(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        ImageSource? image = null;

        try
        {
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

            image?.Freeze();
            return image;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to extract thumbnail for {Path}", path); }

        //Return the value event if null;
        return image;
    }

    #endregion
}