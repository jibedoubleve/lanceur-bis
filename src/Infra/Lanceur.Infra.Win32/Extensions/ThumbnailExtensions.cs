using System.IO;
using System.IO.Hashing;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lanceur.Core.Constants;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Win32.Extensions;

public static class ThumbnailExtensions
{
    #region Fields

    private static readonly object Locker = new();

    #endregion

    #region Methods

    /// <summary>
    ///     Copy the image source into the thumbnail repository. If the
    ///     thumbnail already exits, nothing happens.
    /// </summary>
    /// <param name="imageSource">The image to copy into the repository</param>
    /// <param name="fileName">The file name of the thumbnail</param>
    public static void CopyToImageRepository(this ImageSource imageSource, string fileName)
    {
        if (fileName.IsNullOrWhiteSpace()) { return; }

        var destination = fileName.GetThumbnailAbsolutePath();

        lock (Locker)
        {
            if (File.Exists(destination)) { return; }

            if (!Directory.Exists(Paths.ImageRepository)) { Directory.CreateDirectory(Paths.ImageRepository); }

            if (imageSource is not BitmapSource bitmapSource) { return; }

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using var fileStream = new FileStream(destination, FileMode.Create);
            encoder.Save(fileStream);
        }
    }

    /// <summary>
    ///     Copies the image located at the specified path into the thumbnail repository.
    ///     If a thumbnail with the same name already exists, the method does nothing.
    ///     No copy occurs if the image source or the file name is null, empty, or whitespace.
    /// </summary>
    /// <param name="imageSource">The full path of the source image to copy.</param>
    /// <param name="fileName">The file name to assign to the thumbnail in the repository.</param>
    public static void CopyToImageRepository(this string imageSource, string fileName)
    {
        if (fileName.IsNullOrWhiteSpace()) { return; }

        if (imageSource.IsNullOrWhiteSpace()) { return; }

        var destination = fileName.GetThumbnailAbsolutePath();

        lock (Locker)
        {
            if (File.Exists(destination)) { return; }

            if (!Directory.Exists(Paths.ImageRepository)) { Directory.CreateDirectory(Paths.ImageRepository); }

            File.Copy(imageSource, destination, true);
        }
    }

    /// <summary>
    ///     Resolves the absolute path of a thumbnail file in the image repository.
    /// </summary>
    /// <param name="fileName">The thumbnail file name (without directory).</param>
    /// <returns>
    ///     The absolute path to the thumbnail file, combining <see cref="Paths.ImageRepository" />
    ///     with <paramref name="fileName" />.
    /// </returns>
    public static string GetThumbnailAbsolutePath(this string fileName)
        => Path.Combine(Paths.ImageRepository, fileName);

    /// <summary>
    ///     Computes a deterministic file name for the thumbnail associated with the specified input.
    ///     The name is derived by hashing the input using XxHash64, ensuring a unique and
    ///     collision-resistant file name within the thumbnail repository.
    /// </summary>
    /// <param name="path">The file name or path used as input to generate the thumbnail file name.</param>
    /// <returns>
    ///     A file name (without directory) in the form <c>{hash:x16}.png</c>,
    ///     using the XxHash64 hash of <paramref name="path" />.
    /// </returns>
    public static string GetThumbnailFileName(this string path)
    {
        var bytes = Encoding.UTF8.GetBytes(path);
        var hash = XxHash64.HashToUInt64(bytes);
        return $"{hash:x16}.png";
    }

    #endregion
}