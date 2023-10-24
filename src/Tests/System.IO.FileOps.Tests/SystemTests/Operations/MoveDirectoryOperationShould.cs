using FluentAssertions;
using System.IO.FileOps.Infrastructure;
using System.IO.FileOps.Infrastructure.Operations;

namespace System.IO.FileOps.Test.SystemTests.Operations;

public class MoveDirectoryOperationShould : IDisposable
{
    #region Private members

    private void Cleanup()
    {
        if (Directory.Exists(Source)) Directory.Delete(Source, true);
        if (Directory.Exists(Destination)) Directory.Delete(Destination, true);
    }

    private string Destination { get; }
    private const string DestinationName = "RandomDirectory_dst_MVDIR";

    private string Source { get; }

    private const string SourceName = "RandomDirectory_src_MVDIR";

    #endregion

    #region Constructors

    public MoveDirectoryOperationShould()
    {
        Source = Path.Combine(Path.GetTempPath(), SourceName);

        Cleanup();
        Directory.CreateDirectory(Source);

        using (var fileStream = File.Create(Path.Combine(Source, "output.txt")))
        using (var writer = new StreamWriter(fileStream))
        {
            writer.WriteLine("some random text");
        }

        Destination = Path.Combine(Path.GetTempPath(), DestinationName);
    }

    #endregion

    #region Public methods

    [Fact]
    public async Task BeProcessed()
    {
        // ACT
        var moveDirectory = OperationFactory.MoveDirectory(Source, Destination);
        await moveDirectory.AsOperation()
                           .ProcessAsync();

        // ASSERT
        Directory.Exists(Destination)
                 .Should().BeTrue($"'{Destination}' should be created");

        Directory.EnumerateFiles(Destination)
                 .Count()
                 .Should().BeGreaterThan(0);
    }

    public void Dispose() { Cleanup(); }

    #endregion
}