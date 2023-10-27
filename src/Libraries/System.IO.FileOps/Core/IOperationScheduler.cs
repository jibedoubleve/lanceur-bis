using System.IO.FileOps.Core.Models;
using System.IO.FileOps.Infrastructure;

namespace System.IO.FileOps.Core;

public interface IOperationScheduler
{
    #region Methods

    IOperationScheduler AddOperation(OperationConfiguration operation);

    Task ExecutePlanAsync();

    SchedulerState GetState();

    IOperationScheduler RemoveOperation(OperationInfo operation);

    IOperationScheduler ResetPlan();

    Task SavePlanAsync();

    #endregion Methods
}