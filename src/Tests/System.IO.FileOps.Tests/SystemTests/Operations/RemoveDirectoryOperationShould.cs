using FluentAssertions;
using System.IO.FileOps.Infrastructure;
using System.IO.FileOps.Infrastructure.Operations;

namespace System.IO.FileOps.Test.SystemTests.Operations;

public class RemoveDirectoryOperationShould
{
    #region Methods

    [Fact]
    public async Task BeProcessed()
    {
        // ARRANGE
        var directory = Path.Combine(Path.GetTempPath(), "RandomDirectory");
        Directory.CreateDirectory(directory);

        // ACT
        var rmdir = OperationFactory.RemoveDirectory(directory);
        Directory.Exists(directory).Should().BeTrue();

        await rmdir.AsOperation()
                   .ProcessAsync();

        // ASSERT
        Directory.Exists(directory).Should().BeFalse();
    }

    #endregion Methods
}