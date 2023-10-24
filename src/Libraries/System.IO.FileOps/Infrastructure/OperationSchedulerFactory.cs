using System.IO.FileOps.Core;

namespace System.IO.FileOps.Infrastructure;

public static class OperationSchedulerFactory
{
    #region Public methods

    public static async Task<IOperationScheduler> RetrieveFromFileAsync(string filePath)
    {
        var scheduler = new FileOperationScheduler(filePath);
        await scheduler.LoadFileAsync();
        return scheduler;
    }

    public static IOperationScheduler RetrieveFromMemory() { return new MemoryOperationScheduler(); }

    #endregion
}