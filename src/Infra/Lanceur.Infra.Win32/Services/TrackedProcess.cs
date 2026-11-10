using System.Diagnostics;

namespace Lanceur.Infra.Win32.Services;

/// <summary>
///     The part of a started process needed to track its startup.
/// </summary>
internal interface ITrackedProcess : IDisposable
{
    #region Properties

    /// <summary>
    ///     Gets the exit code of the process. Only available when <see cref="HasExited" /> is <c>true</c>.
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    ///     Gets a value indicating whether the process has exited.
    /// </summary>
    bool HasExited { get; }

    /// <summary>
    ///     Gets the system-assigned identifier (PID) of the process.
    ///     Remains readable after the process has exited, but Windows may reassign it to another process.
    /// </summary>
    int Id { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Waits for the process to be ready to handle user input.
    /// </summary>
    /// <param name="milliseconds">The maximum time to wait, in milliseconds.</param>
    /// <returns><c>true</c> if the process is ready; otherwise, <c>false</c>.</returns>
    bool WaitForInputIdle(int milliseconds);

    #endregion
}

/// <inheritdoc />
internal sealed class TrackedProcess : ITrackedProcess
{
    #region Fields

    private readonly Process _process;

    #endregion

    #region Constructors

    public TrackedProcess(Process process) => _process = process;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int ExitCode => _process.ExitCode;

    /// <inheritdoc />
    public bool HasExited => _process.HasExited;

    /// <inheritdoc />
    public int Id => _process.Id;

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Dispose() => _process.Dispose();

    /// <inheritdoc />
    public bool WaitForInputIdle(int milliseconds) => _process.WaitForInputIdle(milliseconds);

    #endregion
}