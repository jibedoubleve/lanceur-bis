using System.ComponentModel;
using System.Diagnostics;

namespace Lanceur.Infra.Win32.Services;

/// <summary>
///     Default implementation of <see cref="IProcessLookup" />, based on <see cref="Process" />.
/// </summary>
internal class ProcessLookup : IProcessLookup
{
    #region Methods

    /// <inheritdoc />
    public IReadOnlyCollection<int> GetPidsByName(string processName)
    {
        var ps = Process.GetProcessesByName(processName);
        try { return [.. ps.Select(p => p.Id)]; }
        finally
        {
            foreach (var p in ps)
            {
                p.Dispose();
            }
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     PIDs can be reused by Windows: a PID whose process has exited may be
    ///     reassigned to an unrelated process, in which case this returns <c>true</c>.
    /// </remarks>
    public bool IsAlive(int pid)
    {
        try
        {
            using var p = Process.GetProcessById(pid);
            return !p.HasExited;
        }
        catch (ArgumentException) { return false; }
        catch (Win32Exception) { return true; }
    }

    #endregion
}

/// <summary>
///     Queries the running processes of the system. Abstracted so that
///     <see cref="ProcessSnapshot" /> can be unit tested without real processes.
/// </summary>
internal interface IProcessLookup
{
    #region Methods

    /// <summary>
    ///     Returns the PIDs of all the running processes with the specified name
    ///     (without the <c>.exe</c> extension). Returns an empty collection if none is found.
    /// </summary>
    IReadOnlyCollection<int> GetPidsByName(string processName);

    /// <summary>
    ///     Returns <c>true</c> if a process with the specified PID exists and has not exited.
    /// </summary>
    bool IsAlive(int pid);

    #endregion
}