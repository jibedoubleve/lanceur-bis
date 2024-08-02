using FluentAssertions;
using System.IO.FileOps.Core.Models;
using System.IO.FileOps.Infrastructure;

namespace System.IO.FileOps.Test.SystemTests;

public class SchedulerShould : IDisposable
{
    #region Fields

    private const string FilePattern = "lanceur_operation_log_{0}.json";
    private readonly string _fileName = string.Format(FilePattern, Guid.NewGuid());

    #endregion Fields

    #region Methods

    private static List<OperationConfiguration> GetRandomOperations(int count)
    {
        var results = new List<OperationConfiguration>();
        for (var i = 0; i < count; i++)
            results.Add(
                new() { Name = $"NoOperation_{i}", Parameters = new()  { { "1", "un" } } }
            );
        return results;
    }

    [Fact]
    public async Task CreateInMemoryBeforeSaving()
    {
        // ARRANGE
        var scheduler = await OperationSchedulerFactory.RetrieveFromFileAsync(_fileName);
        var operations = GetRandomOperations(4);

        var i = 0;

        // ACT
        scheduler.ResetPlan()
                 .AddOperation(operations[i++])
                 .AddOperation(operations[i++])
                 .AddOperation(operations[i++]);

        // ASSERT
        scheduler.GetState()
                 .OperationCount
                 .Should()
                 .Be(3);
    }

    public void Dispose()
    {
        if (!File.Exists(_fileName)) return;

        File.Delete(_fileName);
    }

    [Fact]
    public async Task RetrievePreviouslyAndAppendOperation()
    {
        // ARRANGE
        var scheduler = await OperationSchedulerFactory.RetrieveFromFileAsync(_fileName);
        var operations = GetRandomOperations(4);

        var i = 0;

        // ACT
        await scheduler.ResetPlan()
                       .AddOperation(operations[i++])
                       .AddOperation(operations[i++])
                       .AddOperation(operations[i++])
                       .SavePlanAsync();

        var scheduler2 = await OperationSchedulerFactory.RetrieveFromFileAsync(_fileName);
        await scheduler2.AddOperation(operations[i++])
                        .SavePlanAsync();

        var retrieved = await OperationSchedulerFactory.RetrieveFromFileAsync(_fileName);

        // ASSERT
        retrieved.GetState()
                 .OperationCount
                 .Should()
                 .Be(4);
    }

    [Fact]
    public async Task RetrievePreviouslySavedScheduler()
    {
        // ARRANGE
        var scheduler = await OperationSchedulerFactory.RetrieveFromFileAsync(_fileName);
        var operations = GetRandomOperations(4);

        var i = 0;

        // ACT
        await scheduler.ResetPlan()
                       .AddOperation(operations[i++])
                       .AddOperation(operations[i++])
                       .AddOperation(operations[i++])
                       .SavePlanAsync();

        // ASSERT
        var retrieved = await OperationSchedulerFactory.RetrieveFromFileAsync(_fileName);
        retrieved.GetState()
                 .OperationCount
                 .Should()
                 .Be(3);
    }

    #endregion Methods
}