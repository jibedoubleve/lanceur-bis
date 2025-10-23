using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lanceur.Infra.Win32.Images;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public class ThumbnailShould
{
    #region Methods

    private static IEnumerable<object> NotFailWhenCopyingImageSourceThumbnailSource()
    {
        yield return new object[] { null, Path.GetTempPath() };
        yield return new object[] { new BitmapImage(), null };
        yield return new object[] { null, null };
    }

    private static IEnumerable<object> NotFailWhenCopyingThumbnailSource()
    {
        yield return new object[] { null, Path.GetTempPath() };
        yield return new object[] { Path.GetTempFileName(), null };
        yield return new object[] { null, null };
    }

    [Theory]
    [InlineData((string)null)]
    public void NotFailsWhenCopyThumbnailToImageSourceRepository(string destinationPath)
    {
        var thumbnailPath = new BitmapImage();
        var action = () => thumbnailPath.CopyToImageRepository(destinationPath);
        action.ShouldNotThrow();
    }

    [Theory]
    [MemberData(nameof(NotFailWhenCopyingThumbnailSource))]
    public void NotFailsWhenCopyThumbnailToRepository(string thumbnailPath, string destinationPath)
    {
        var action = () => thumbnailPath.CopyToImageRepository(destinationPath);
        action.ShouldNotThrow();
    }

    [StaTheory]
    [MemberData(nameof(NotFailWhenCopyingImageSourceThumbnailSource))]
    public void NotFailWhenCopyingImageSourceThumbnail(ImageSource thumbnail, string destinationPath)
    {
        var action = () => thumbnail.CopyToImageRepository(destinationPath);
        action.ShouldNotThrow();
    }

    #endregion
}