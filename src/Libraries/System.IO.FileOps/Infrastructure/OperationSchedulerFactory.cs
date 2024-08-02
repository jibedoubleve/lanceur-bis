using System.IO.FileOps.Core;

namespace System.IO.FileOps.Infrastructure;

public static class OperationSchedulerFactory
{
    #region Methods

    public static async Task<IOperationScheduler> RetrieveFromFileAsync(string filePath)
    {
        var scheduler = new FileOperationScheduler(filePath);
        await scheduler.LoadFileAsync();
        return scheduler;
    }

    public static IOperationScheduler RetrieveFromMemory() => new MemoryOperationScheduler();

    #endregion Methods
}