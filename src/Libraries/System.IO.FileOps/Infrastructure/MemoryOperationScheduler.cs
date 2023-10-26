namespace System.IO.FileOps.Infrastructure;

internal class MemoryOperationScheduler : AbstractOperationScheduler
{
    #region Methods

    public override Task SavePlanAsync()
    { return Task.CompletedTask; }

    #endregion Methods
}