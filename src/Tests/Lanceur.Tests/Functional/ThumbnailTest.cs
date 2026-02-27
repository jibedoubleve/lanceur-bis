using System.Windows.Media;
using System.Windows.Media.Imaging;
using Lanceur.Infra.Win32.Extensions;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public class ThumbnailTest
{
    #region Methods

    private static IEnumerable<object> SourceDestinationBitmapSource()
    {
        yield return new object[] { null, Path.GetTempPath() };
        yield return new object[] { new BitmapImage(), null };
        yield return new object[] { null, null };
    }

    private static IEnumerable<object> SourceDestinationPathSource()
    {
        yield return new object[] { null, Path.GetTempPath() };
        yield return new object[] { Path.GetTempFileName(), null };
        yield return new object[] { null, null };
    }

    [Theory]
    [InlineData((string)null)]
    public void When_copy_thumbnail_to_ImageSourceRepository_Then_no_error(string destinationPath)
    {
        var thumbnailPath = new BitmapImage();
        var action = () => thumbnailPath.CopyToImageRepository(destinationPath);
        action.ShouldNotThrow();
    }

    [Theory]
    [MemberData(nameof(SourceDestinationPathSource))]
    public void When_copy_thumbnail_to_repository_Then_no_error(string thumbnailPath, string destinationPath)
    {
        var action = () => thumbnailPath.CopyToImageRepository(destinationPath);
        action.ShouldNotThrow();
    }

    [StaTheory]
    [MemberData(nameof(SourceDestinationBitmapSource))]
    public void When_copying_ImageSourceThumbnail_Then_no_error(ImageSource thumbnail, string destinationPath)
    {
        var action = () => thumbnail.CopyToImageRepository(destinationPath);
        action.ShouldNotThrow();
    }

    #endregion
}