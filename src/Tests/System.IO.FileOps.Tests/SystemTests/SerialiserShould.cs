using FluentAssertions;
using Newtonsoft.Json;
using System.IO.FileOps.Infrastructure;
using System.IO.FileOps.Infrastructure.Operations;
using System.IO.FileOps.Test.Helpers;
using Xunit.Abstractions;

namespace System.IO.FileOps.Test.SystemTests;

public class SerialiserShould : IDisposable
{
    #region Fields

    private readonly string _destinationDir = Path.Combine(Path.GetTempPath(), "destination");
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "toRemove");

    private readonly string _jsonFile = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.json");

    private readonly ITestOutputHelper _output;
    private readonly string _zipFile = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.zip");

    #endregion Fields

    #region Constructors

    public SerialiserShould(ITestOutputHelper output)
    {
        _output = output;

        _output.WriteLine($"Zip file       : {_zipFile}");
        _output.WriteLine($"Destination dir: {_destinationDir}");
        _output.WriteLine($"Directory      : {_directory}");
    }

    #endregion Constructors

    #region Methods

    private void OutputJsonFile()
    {
        if (!File.Exists(_jsonFile))
        {
            _output.WriteLine($"File '{_jsonFile}' does not exist.");
            return;
        }

        var json = File.ReadAllText(_jsonFile);
        json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
        _output.WriteLine("Json configuration file contains:");
        _output.WriteLine(json);
    }

    [Fact]
    public async Task Deserialize()
    {
        var scheduler = await OperationSchedulerFactory.RetrieveFromFileAsync(_jsonFile);

        Directory.CreateDirectory(_destinationDir);

        await scheduler.AddOperation(OperationFactory.RemoveDirectory(_destinationDir))
                       .AddOperation(OperationFactory.UnzipDirectory(_zipFile, _destinationDir))
                       .SavePlanAsync();

        var deserializedScheduler = await OperationSchedulerFactory.RetrieveFromFileAsync(_jsonFile);
        deserializedScheduler.GetState()
                             .OperationCount
                             .Should()
                             .Be(2);
    }

    public void Dispose()
    {
        if (File.Exists(_jsonFile)) File.Delete(_jsonFile);
        if (File.Exists(_zipFile)) File.Delete(_zipFile);

        if (Directory.Exists(_destinationDir)) Directory.Delete(_destinationDir, true);
        if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
    }

    [Fact]
    public async Task Serialize()
    {
        // ARRANGE
        if (!Directory.Exists(_destinationDir)) Directory.CreateDirectory(_destinationDir);
        if (!Directory.Exists(_directory)) Directory.CreateDirectory(_directory);

        var textFile = Path.Combine(_directory, "some.txt");
        await File.WriteAllTextAsync(textFile, "Some random text");
        ZipHelper.Zip(textFile, _zipFile);
        File.Delete(textFile);

        var scheduler = await OperationSchedulerFactory.RetrieveFromFileAsync(_jsonFile);
        await scheduler.AddOperation(OperationFactory.RemoveDirectory(_directory))
                       .AddOperation(OperationFactory.UnzipDirectory(_zipFile, _destinationDir))
                       .SavePlanAsync();

        OutputJsonFile();

        // ACT
        var sut = await OperationSchedulerFactory.RetrieveFromFileAsync(_jsonFile);
        await sut.ExecutePlanAsync();

        // ASSERT
        Directory.Exists(_directory)
                 .Should()
                 .BeFalse();

        File.Exists(_jsonFile)
            .Should()
            .BeTrue($"'{_jsonFile}' should exist");

        Directory.GetFiles(_destinationDir)
                 .Length
                 .Should()
                 .BeGreaterThan(0);
    }

    #endregion Methods
}