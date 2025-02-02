using Lanceur.Infra.Constants;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lanceur.Infra.Win32.Images;

public static class ThumbnailExtensions
{
    #region Fields

    private static readonly object Locker = new();

    #endregion Fields

    #region Methods

    /// <summary>
    /// Copy the image source into the thumbnail repository. If the
    /// thumbnail already exits, nothing happens.
    /// </summary>
    /// <param name="imageSource">The image to copy into the repository</param>
    /// <param name="fileName">The file name of the thumbnail</param>
    public static void CopyToImageRepository(this ImageSource imageSource, string fileName)
    {
        var destination = fileName.GetThumbnailPath();

        lock (Locker)
        {
            if (File.Exists(destination)) return;

            if (!Directory.Exists(Paths.ImageRepository)) Directory.CreateDirectory(Paths.ImageRepository);
            if (imageSource is not BitmapSource bitmapSource) return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using var fileStream = new FileStream(destination, FileMode.Create);
            encoder.Save(fileStream);
        }
    }

    /// <summary>
    /// Copy the image specified by the path into the thumbnail repository. If the
    /// thumbnail already exits, nothing happens.
    /// </summary>
    /// <param name="imageSource">The image to copy into the repository</param>
    /// <param name="fileName">The file name of the thumbnail</param>
    public static void CopyToImageRepository(this string imageSource, string fileName)
    {
        var destination = fileName.GetThumbnailPath();

        if (File.Exists(destination)) return;

        lock (Locker)
        {
            if (!Directory.Exists(Paths.ImageRepository)) Directory.CreateDirectory(Paths.ImageRepository);
            File.Copy(imageSource, destination, true);
        }
    }

    /// <summary>
    /// Generates the absolute path for the thumbnail image associated with the specified file name.
    /// </summary>
    /// <param name="fileName">The file name, possibly containing a prefix like "package:", for which to generate the thumbnail path.</param>
    /// <returns>The absolute path to the thumbnail image corresponding to the given file name.</returns>
    public static string GetThumbnailPath(this string fileName) 
        => Path.Combine(Paths.ImageRepository, $"{fileName.Replace("package:", "")}.ico");

    #endregion Methods
}