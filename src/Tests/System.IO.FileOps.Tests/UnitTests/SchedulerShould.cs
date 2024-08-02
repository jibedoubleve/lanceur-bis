using FluentAssertions;
using System.IO.FileOps.Core.Models;
using System.IO.FileOps.Infrastructure;

namespace System.IO.FileOps.Test.UnitTests;

public class SchedulerShould
{
    #region Methods

    [Fact]
    public async Task CreateOperation_WhenSaved()
    {
        // ARRANGE
        var operation1 = new OperationConfiguration { Name = "operation1" };
        var operation2 = new OperationConfiguration { Name = "operation2" };
        var operation3 = new OperationConfiguration { Name = "operation3" };

        // ACT
        var scheduler = OperationSchedulerFactory.RetrieveFromMemory();
        await scheduler.ResetPlan()
                       .AddOperation(operation1)
                       .AddOperation(operation2)
                       .AddOperation(operation3)
                       .SavePlanAsync();

        // ASSERT
        scheduler.GetState()
                 .OperationCount
                 .Should()
                 .Be(3);
    }

    #endregion Methods
}