using FluentAssertions;
using System.IO.FileOps.Infrastructure;
using System.IO.FileOps.Infrastructure.Operations;
using System.IO.FileOps.Test.Helpers;
using Xunit.Abstractions;

namespace System.IO.FileOps.Test.SystemTests.Operations;

public class UnzipDirectoryOperationShould : IDisposable
{
    #region Fields

    private const string ArchiveFileName = "Package.zip";
    private const string DestinationDirectoryName = "RandomDirectory_dst_ZIP";
    private const string SourceDirectoryName = "RandomDirectory_src_ZIP";
    private const string TextFileName = "random_text_file.txt";

    #endregion Fields

    #region Constructors

    public UnzipDirectoryOperationShould(ITestOutputHelper output)
    {
        Source = Path.Combine(Path.GetTempPath(), SourceDirectoryName);
        Destination = Path.Combine(Path.GetTempPath(), DestinationDirectoryName);
        ArchiveFile = Path.Combine(Path.GetTempPath(), Destination, ArchiveFileName);

        Cleanup();

        Directory.CreateDirectory(Source);
        Directory.CreateDirectory(Destination);

        var outfile = Path.Combine(Source, TextFileName);

        output.WriteLine($"Source dir     : '{Source}'");
        output.WriteLine($"Destination dir: '{Destination}'");
        output.WriteLine($"Archive dir    : '{ArchiveFile}'");

        ZipHelper.Zip(outfile, ArchiveFile);
    }

    #endregion Constructors

    #region Properties

    private string ArchiveFile { get; }
    private string Destination { get; }

    private string Source { get; }

    #endregion Properties

    #region Methods

    private void Cleanup()
    {
        if (Directory.Exists(Source)) Directory.Delete(Source, true);
        if (Directory.Exists(Destination)) Directory.Delete(Destination, true);
        if (File.Exists(ArchiveFile)) File.Delete(ArchiveFile);
    }

    [Fact]
    public async Task BeProcessed()
    {
        // ACT
        var unzipDir = OperationFactory.UnzipDirectory(ArchiveFile, Destination);
        await unzipDir.ToOperation()
                      .ProcessAsync();

        // ASSERT
        var path = Path.Combine(Destination, TextFileName);
        Directory.EnumerateFiles(Destination)
                 .Count(f => f == path)
                 .Should().BeGreaterThan(0);
    }

    public void Dispose()
    { Cleanup(); }

    #endregion Methods
}