using System.IO;
using System.IO.Hashing;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Win32.Extensions;

public static class ThumbnailExtensions
{
    #region Fields

    private static readonly object Locker = new();

    #endregion

    #region Methods

    /// <summary>
    ///     Computes a deterministic hash-based file name from the specified text.
    ///     Uses XxHash64 to produce a unique, collision-resistant name suitable
    ///     for use as a thumbnail file name in the image repository.
    /// </summary>
    /// <param name="source">The text to hash (e.g. a file path or any unique identifier).</param>
    /// <returns>
    ///     A file name in the form <c>{hash:x16}.png</c>.
    /// </returns>
    private static string ComputeHash(this string source)
    {
        var bytes = Encoding.UTF8.GetBytes(source);
        var hash = XxHash64.HashToUInt64(bytes);
        return $"{hash:x16}.png";
    }

    /// <summary>
    ///     Copies the image source into the thumbnail repository.
    ///     Does nothing if <paramref name="outputPath" /> is null, empty, or does not resolve
    ///     to a path within <see cref="Paths.ImageRepository" />, or if the thumbnail already exists.
    /// </summary>
    /// <param name="imageSource">The image to save into the repository.</param>
    /// <param name="outputPath">The thumbnail file name (without directory).</param>
    public static void CopyToImageRepository(this ImageSource imageSource, string? outputPath)
    {
        if (outputPath.IsNullOrWhiteSpace()) { return; }

        var destination = Path.Combine(Paths.ImageRepository, outputPath!);
        if (!destination.StartsWith(Paths.ImageRepository)) { return; }

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
    ///     Does nothing if <paramref name="outputPath" /> or <paramref name="imageSource" />
    ///     is null, empty, or whitespace, or if the thumbnail already exists.
    /// </summary>
    /// <param name="imageSource">The absolute path of the source image to copy.</param>
    /// <param name="outputPath">The absolute destination path within <see cref="Paths.ImageRepository" />.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if <paramref name="outputPath" /> does not point to <see cref="Paths.ImageRepository" />,
    ///     to prevent accidental writes outside the thumbnail repository.
    /// </exception>
    public static void CopyToImageRepository(this string imageSource, string outputPath)
    {
        if (outputPath.IsNullOrWhiteSpace()) { return; }

        if (imageSource.IsNullOrWhiteSpace()) { return; }

        if (!outputPath.StartsWith(Paths.ImageRepository))
        {
            throw new InvalidOperationException(
                $"Cannot save the thumbnail to this repository: {outputPath}!"
            );
        }

        lock (Locker)
        {
            if (File.Exists(outputPath)) { return; }

            if (!Directory.Exists(Paths.ImageRepository)) { Directory.CreateDirectory(Paths.ImageRepository); }

            File.Copy(imageSource, outputPath, true);
        }
    }

    /// <summary>
    ///     Resolves the absolute path of the thumbnail for the specified alias.
    ///     The path is derived by hashing <see cref="AliasQueryResult.FileName" /> via <see cref="ComputeHash" />
    ///     and combining the result with <see cref="Paths.ImageRepository" />.
    ///     Returns <see cref="string.Empty" /> if <see cref="AliasQueryResult.FileName" /> is null or whitespace.
    /// </summary>
    /// <param name="alias">The alias for which to resolve the thumbnail absolute path.</param>
    /// <returns>
    ///     The absolute path to the thumbnail file, or <see cref="string.Empty" />
    ///     if the alias has no file name.
    /// </returns>
    public static string GetThumbnailAbsolutePath(this AliasQueryResult alias)
    {
        var fileName = alias.FileName.IsNullOrWhiteSpace()
            ? string.Empty
            : alias.FileName!.ComputeHash();
        return Path.Combine(Paths.ImageRepository, fileName);
    }

    #endregion
}