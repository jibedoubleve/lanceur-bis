using Lanceur.Infra.Win32.Services;

namespace Lanceur.Tests.Tools.Stubs;

public sealed class FakeProcessLookup(params int[] alive) : IProcessLookup
{
    #region Fields

    private readonly HashSet<int> _alive = [.. alive];

    #endregion

    #region Properties

    public int IsAliveCalls { get; private set; }

    #endregion

    #region Methods

    public IReadOnlyCollection<int> GetPidsByName(string processName) => [.. _alive];

    public bool IsAlive(int pid)
    {
        IsAliveCalls++;
        return _alive.Contains(pid);
    }

    #endregion
}