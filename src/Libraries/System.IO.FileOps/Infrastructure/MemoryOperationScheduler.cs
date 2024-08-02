namespace System.IO.FileOps.Infrastructure;

internal class MemoryOperationScheduler : AbstractOperationScheduler
{
    #region Methods

    public override Task SavePlanAsync() => Task.CompletedTask;

    #endregion Methods
}