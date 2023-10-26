using System.IO.FileOps.Core;

namespace System.IO.FileOps.Infrastructure.Operations;

internal class RemoveDirectoryOperation : AbstractOperation, IOperation
{
    #region Constructors

    public RemoveDirectoryOperation(Dictionary<string, string> parameters) : base("rmdir", parameters)
    {
    }

    #endregion Constructors

    #region Methods

    public Task ProcessAsync()
    {
        var directory = Parameters["directory"];
        if (!Directory.Exists(directory)) return Task.CompletedTask;

        Directory.Delete(directory, true);
        return Task.CompletedTask;
    }

    #endregion Methods
}