using System.IO.FileOps.Core;
using System.IO.FileOps.Core.Models;

namespace System.IO.FileOps.Infrastructure;

internal abstract class AbstractOperationScheduler : IOperationScheduler
{
    #region Fields

    private readonly List<OperationConfiguration> _operations = new();

    #endregion Fields

    #region Properties

    protected IEnumerable<OperationConfiguration> Operations => _operations.ToArray();

    #endregion Properties

    #region Methods

    private static IEnumerable<IOperation> GetOperations(IEnumerable<OperationConfiguration> configurations) { return configurations.Select(cfg => cfg.ToOperation()).ToList(); }

    protected IOperationScheduler AddOperations(IEnumerable<OperationConfiguration> operations, bool resetList = true)
    {
        if (resetList) _operations.Clear();

        var array = operations as OperationConfiguration[] ?? operations.ToArray();
        if (array.Intersect(_operations).Any()) return this;

        _operations.AddRange(array);
        return this;
    }

    public IOperationScheduler AddOperation(OperationConfiguration operation)
    {
        if (_operations.Any(x => x == operation)) return this;

        _operations.Add(operation);
        return this;
    }

    public async Task ExecutePlanAsync()
    {
        var ops = GetOperations(_operations);
        foreach (var op in ops) await op.ProcessAsync();
    }

    public SchedulerState GetState() => new() { OperationCount = _operations.Count };

    public IOperationScheduler RemoveOperation(OperationInfo operationInfo)
    {
        var toDel = (from op in _operations
                     where op.Name == operationInfo.Name && op.Parameters.ContainsKey(operationInfo.Key) && op.Parameters[operationInfo.Key] == operationInfo.Value
                     select op).FirstOrDefault();

        if (toDel is not null) _operations.Remove(toDel);

        return this;
    }

    public IOperationScheduler ResetPlan()
    {
        _operations.Clear();
        return this;
    }

    public abstract Task SavePlanAsync();

    #endregion Methods
}