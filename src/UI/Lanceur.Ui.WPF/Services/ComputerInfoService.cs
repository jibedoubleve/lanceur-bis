using System.Runtime.InteropServices;
using Lanceur.Core.Services;

namespace Lanceur.Ui.WPF.Services;

public class ComputerInfoService : IComputerInfoService
{
    #region Properties

    /// <inheritdoc />
    public bool IsMonitoring { get; private set; }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task StartMonitoring(TimeSpan interval, Action<(double CpuLoad, double MemoryLoad)> callback)
    {
        IsMonitoring = true;

        await Task.Run(() =>
            {
                while (true)
                {
                    if (!IsMonitoring) break;

                    var cpuLoadTask = SystemMetrics.GetCpuUsagePercentAsync((int)interval.TotalMilliseconds);
                    var memoryLoad = SystemMetrics.GetMemoryUse();
                    var delay = Task.Delay(interval);

                    Task.WaitAll(cpuLoadTask, delay);

                    callback((cpuLoadTask.Result, memoryLoad));
                }
            }
        );
    }

    /// <inheritdoc />
    public void StopMonitoring() { IsMonitoring = false; }

    #endregion
}

internal static class SystemMetrics
{
    #region Methods

    // -------- CPU --------  
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out Filetime idle, out Filetime kernel, out Filetime user);

    // -------- Memory --------  
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    private static CpuSample SampleCpu()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
            throw new InvalidOperationException("GetSystemTimes failed.");

        return new(ToUInt64(idle), ToUInt64(kernel), ToUInt64(user));
    }

    private static ulong ToUInt64(Filetime ft) => ((ulong)ft.dwHighDateTime << 32) | ft.dwLowDateTime;

    // Returns CPU usage [%] between two samples  
    public static double CpuUsagePercent(in CpuSample start, in CpuSample end)
    {
        var idleDelta = end.Idle - start.Idle;
        var kernelDelta = end.Kernel - start.Kernel;
        var userDelta = end.User - start.User;

        // kernel includes idle, so subtract idle from kernel  
        var busy = kernelDelta - idleDelta + userDelta;
        var total = kernelDelta + userDelta;

        if (total == 0) return 0;

        var pct = 100.0 * busy / total;
        if (pct < 0) return 0;
        if (pct > 100) return 100;

        return pct;
    }

    // Convenience: sample across a short interval  
    public static async Task<double> GetCpuUsagePercentAsync(int delayMs = 500, CancellationToken ct = default)
    {
        var a = SampleCpu();
        await Task.Delay(delayMs, ct).ConfigureAwait(false);
        var b = SampleCpu();
        return CpuUsagePercent(a, b);
    }

    public static uint GetMemoryUse()
    {
        var ms = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref ms)) throw new InvalidOperationException("GlobalMemoryStatusEx failed.");

        return ms.dwMemoryLoad;
    }

    #endregion

    [StructLayout(LayoutKind.Sequential)]
    private struct Filetime
    {
        #region Fields

        public uint dwHighDateTime;
        public uint dwLowDateTime;

        #endregion
    }

    public readonly struct CpuSample
    {
        #region Fields

        public readonly ulong Idle;
        public readonly ulong Kernel;
        public readonly ulong User;

        #endregion

        #region Constructors

        public CpuSample(ulong idle, ulong kernel, ulong user)
        {
            Idle = idle;
            Kernel = kernel;
            User = user;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        #region Fields

        public uint dwLength;
        public uint dwMemoryLoad;     // % of physical memory in use (approx)  
        public ulong ullAvailExtendedVirtual;
        public ulong ullAvailPageFile;
        public ulong ullAvailPhys;
        public ulong ullAvailVirtual;
        public ulong ullTotalPageFile;
        public ulong ullTotalPhys;
        public ulong ullTotalVirtual;

        #endregion
    }
}