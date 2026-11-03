using Lanceur.Infra.Win32.Services;

namespace Lanceur.Tests.Tools.Fakes;

/// <summary>
///     Replays the given readiness results, one per wait, and records the delay of each wait.
///     When the process has exited, a wait throws as <see cref="System.Diagnostics.Process" /> does.
/// </summary>
public sealed class FakeProcess : ITrackedProcess
{
    #region Fields

    public const int NewPid = 200;
    public const int OldPid = 100;

    private readonly Queue<bool> _readiness;

    #endregion

    #region Constructors

    public FakeProcess(params bool[] readiness) => _readiness = new Queue<bool>(readiness);

    public FakeProcess(int id) : this() => Id = id;

    #endregion

    #region Properties

    public int ExitCode => 0;

    public bool HasExited { get; init; }
    public int Id { get; init; } = NewPid;

    public List<int> Waits { get; } = [];

    #endregion

    #region Methods

    public void Dispose() { }

    public bool WaitForInputIdle(int milliseconds)
    {
        if (HasExited) { throw new InvalidOperationException("Process has exited."); }

        Waits.Add(milliseconds);
        return _readiness.Dequeue();
    }

    #endregion
}