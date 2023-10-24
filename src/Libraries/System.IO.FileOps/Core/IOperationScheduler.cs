using System.IO.FileOps.Core.Models;
using System.IO.FileOps.Infrastructure;

namespace System.IO.FileOps.Core;

public interface IOperationScheduler
{
    #region Public methods

    IOperationScheduler AddOperation(OperationConfiguration operation);
    IOperationScheduler RemoveOperation(OperationInfo operation);
    Task ExecutePlanAsync();

    SchedulerState GetState();

    IOperationScheduler ResetPlan();

    Task SavePlanAsync();

    #endregion
}