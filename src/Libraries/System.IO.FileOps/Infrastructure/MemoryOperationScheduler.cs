namespace System.IO.FileOps.Infrastructure;

internal class MemoryOperationScheduler : AbstractOperationScheduler
{
    #region Public methods

    public override Task SavePlanAsync() { return Task.CompletedTask; }

    #endregion
}