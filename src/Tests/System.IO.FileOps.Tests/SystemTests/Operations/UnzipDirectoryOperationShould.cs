using System.IO.FileOps.Test.Helpers;
using FluentAssertions;
using System.IO.FileOps.Infrastructure;
using System.IO.FileOps.Infrastructure.Operations;
using Xunit.Abstractions;

namespace System.IO.FileOps.Test.SystemTests.Operations;

public class UnzipDirectoryOperationShould : IDisposable
{
    #region Private members

    private string ArchiveFile { get; }

    private const string ArchiveFileName = "Package.zip";

    private void Cleanup()
    {
        if (Directory.Exists(Source)) Directory.Delete(Source, true);
        if (Directory.Exists(Destination)) Directory.Delete(Destination, true);
        if (File.Exists(ArchiveFile)) File.Delete(ArchiveFile);
    }

    private string Destination { get; }
    private const string DestinationDirectoryName = "RandomDirectory_dst_ZIP";

    private string Source { get; }
    private const string SourceDirectoryName = "RandomDirectory_src_ZIP";
    private const string TextFileName = "random_text_file.txt";

    #endregion

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

    #endregion

    #region Public methods

    [Fact]
    public async Task BeProcessed()
    {
        // ACT
        var unzipDir = OperationFactory.UnzipDirectory(ArchiveFile, Destination);
        await unzipDir.AsOperation()
                      .ProcessAsync();

        // ASSERT
        var path = Path.Combine(Destination, TextFileName);
        Directory.EnumerateFiles(Destination)
                 .Count(f => f == path)
                 .Should().BeGreaterThan(0);
    }

    public void Dispose() { Cleanup(); }

    #endregion
}