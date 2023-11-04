using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Mixins;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lanceur.Infra.Win32.Images;

public static class ThumbnailMixin
{
    #region Fields

    private static readonly object _locker = new();

    #endregion Fields

    #region Methods

    public static void CopyToCache(this ImageSource imageSource, string fileName)
    {
        lock (_locker)
        {
            var cachePath = AppPaths.ImageCache.ExpandPath();
            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);

            if (imageSource is not BitmapSource bitmapSource) return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            fileName = Path.Combine(cachePath, fileName);
            using var fileStream = new FileStream(fileName, FileMode.Create);
            encoder.Save(fileStream);
        }
    }

    public static void CopyToCache(this string source, string output)
    {
        lock (_locker)
        {
            output = $"{output.Replace("package:", "")}.png";
            var cachePath = AppPaths.ImageCache.ExpandPath();
            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);

            var destination = Path.Combine(cachePath, output);
            if (File.Exists(destination)) return;
            if (File.Exists(source))
            {
                File.Copy(source, destination, true);
            }
        }
    }

    #endregion Methods
}