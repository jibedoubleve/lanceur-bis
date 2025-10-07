namespace Lanceur.Core.Services;

public interface IComputerInfoService
{
    #region Properties

    /// <summary>
    ///     Indicates whether the system monitoring process is currently running.
    /// </summary>
    bool IsMonitoring { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Starts monitoring system performance metrics at a specified interval.
    /// </summary>
    /// <param name="interval">Interval in milliseconds between each monitoring update.</param>
    /// <param name="callback">Callback function that receives the current CPU load, total memory, and available memory.</param>
    Task StartMonitoring(TimeSpan interval, Action<(double CpuLoad, double MemoryLoad)> callback);

    /// <summary>
    ///     Stops the system monitoring process.
    /// </summary>
    void StopMonitoring();

    #endregion
}