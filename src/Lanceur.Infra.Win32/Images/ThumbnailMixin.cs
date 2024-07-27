using Lanceur.Infra.Constants;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lanceur.Infra.Win32.Images;

public static class ThumbnailMixin
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
        var destination = fileName.ToAbsolutePath();

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
        var destination = fileName.ToAbsolutePath();

        if (File.Exists(destination)) return;

        lock (Locker)
        {
            if (!Directory.Exists(Paths.ImageRepository)) Directory.CreateDirectory(Paths.ImageRepository);
            File.Copy(imageSource, destination, true);
        }
    }

    /// <summary>
    /// With the <c>alias.Name</c>, it'll get the path where the thumbnail
    /// should be or should be saved.
    /// </summary>
    /// <param name="fileName">The <c>alias.Name</c></param>
    /// <returns>The absolute path to the thumbnail of the specified <c>alias.Name</c></returns>
    public static string ToAbsolutePath(this string fileName) => Path.Combine(Paths.ImageRepository, $"{fileName.Replace("package:", "")}.png");

    #endregion Methods
}