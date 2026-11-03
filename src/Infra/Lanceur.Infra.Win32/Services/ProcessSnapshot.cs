using System.Diagnostics;

namespace Lanceur.Infra.Win32.Services;

/// <summary>
///     Tracks a set of process IDs (PIDs) captured at a given moment, and tells
///     whether any of them is still running.
/// </summary>
/// <remarks>
///     Typical use: call <see cref="Capture" /> before starting a process to record
///     the instances already running, then <see cref="Add" /> the PID of the newly
///     started process, and poll <see cref="AnyAlive" /> to check whether any of them is still alive.
/// </remarks>
internal sealed class ProcessSnapshot
{
    #region Fields

    private readonly IProcessLookup _lookup;

    /// <summary>
    ///     PIDs tracked by this snapshot (the ones captured initially plus any added afterwards).
    /// </summary>
    private readonly HashSet<int> _pids;

    #endregion

    #region Constructors

    /// <summary>
    ///     Creates a snapshot tracking the specified PIDs.
    /// </summary>
    /// <param name="lookup">Used to check whether a process is still running.</param>
    /// <param name="pids">The initial PIDs to track. Duplicates are ignored.</param>
    public ProcessSnapshot(IProcessLookup lookup, IEnumerable<int> pids)
    {
        _lookup = lookup;
        _pids = [.. pids];
    }

    #endregion

    #region Properties

    /// <summary>
    ///     <c>true</c> if the snapshot tracks no PID at all.
    /// </summary>
    public bool IsEmpty => _pids.Count == 0;

    #endregion

    #region Methods

    /// <summary>
    ///     Adds a PID to the tracked set. Does nothing if it is already tracked.
    /// </summary>
    public void Add(int pid) => _pids.Add(pid);

    /// <summary>
    ///     Returns <c>true</c> if at least one of the tracked processes is still running.
    /// </summary>
    /// <remarks>Queries the system on every call; the result is not cached.</remarks>
    public bool AnyAlive() => _pids.Any(_lookup.IsAlive);

    /// <summary>
    ///     Creates a snapshot of all the processes currently running with the specified name.
    /// </summary>
    /// <param name="lookup">Used to find the processes and check their state later.</param>
    /// <param name="processName">
    ///     The process name without the <c>.exe</c> extension (e.g. <c>notepad</c>),
    ///     as expected by <see cref="Process.GetProcessesByName(string)" />.
    /// </param>
    public static ProcessSnapshot Capture(IProcessLookup lookup, string processName) =>
        new(lookup, lookup.GetPidsByName(processName));

    #endregion
}