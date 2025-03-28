using System.Diagnostics;
using Lanceur.Core.Services;
using Microsoft.VisualBasic.Devices;

namespace Lanceur.Ui.WPF.Services;

public class ComputerInfoService : IComputerInfoService
{
    #region Fields

    public float _totalPhysicalMemory;

    private readonly PerformanceCounter _cpuLoad = new("Processor", "% Processor Time", "_Total");
    private readonly PerformanceCounter _memoryUsage = new("Memory", "Available MBytes");

    private static readonly ComputerInfo ComputerInfo = new();

    #endregion

    #region Constructors

    public ComputerInfoService() => _totalPhysicalMemory = ComputerInfo.TotalPhysicalMemory / (1024 * 1024);

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsMonitoring { get; private set; }

    #endregion

    #region Methods

    private (int CpuLoad, int MemoryLoad) GetSystemMetrics()
    {
        var memoryUsage = _memoryUsage.NextValue();
        var memoryLoad = (1 - memoryUsage / _totalPhysicalMemory) * 100;

        var cpuLoad = _cpuLoad.NextValue();

        return (
            CpuLoad: (int)cpuLoad,
            MemoryLoad: (int)memoryLoad
        );
    }

    /// <inheritdoc />
    public async Task StartMonitoring(TimeSpan interval, Action<(int CpuLoad, int MemoryLoad)> callback)
    {
        IsMonitoring = true;
        while (true)
        {
            if (!IsMonitoring) break;

            callback(GetSystemMetrics());
            await Task.Delay(interval);
        }
    }

    /// <inheritdoc />
    public void StopMonitoring() { IsMonitoring = false; }

    #endregion
}