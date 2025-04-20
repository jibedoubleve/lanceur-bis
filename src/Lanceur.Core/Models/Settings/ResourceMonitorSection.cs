namespace Lanceur.Core.Models.Settings;

public class ResourceMonitorSection
{
    #region Constructors

    public ResourceMonitorSection(int cpuSmoothingIndex, int refreshRate)
    {
        CpuSmoothingIndex = cpuSmoothingIndex;
        RefreshRate = refreshRate;
    }

    #endregion

    #region Properties
    

    /// <summary>
    ///     Defines the smoothing index for the CPU monitor.
    ///     A higher value results in a smoother but less responsive graph.
    ///     Default value is 10.
    /// </summary>
    public int CpuSmoothingIndex { get; set; }

    /// <summary>
    ///     Specifies the refresh rate of the resource monitor in milliseconds.
    ///     Determines how frequently the CPU usage data is updated.
    ///     Default value is 500ms.
    /// </summary>
    public int RefreshRate { get; set; }

    #endregion
}