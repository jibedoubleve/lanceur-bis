using System.Collections.Concurrent;

namespace Lanceur.Infra.Win32.Helpers;

/// <inheritdoc />
public sealed class StaThreadRunner : IStaThreadRunner
{
    #region Fields

    private readonly BlockingCollection<Action> _threadQueue = new();

    #endregion

    #region Constructors

    public StaThreadRunner()
    {
        var staThread = new Thread(() =>
            {
                foreach (var action in _threadQueue.GetConsumingEnumerable()) action.Invoke();
            }
        );
        staThread.SetApartmentState(ApartmentState.STA);
        staThread.IsBackground = true;
        staThread.Start();
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Dispose() => _threadQueue.CompleteAdding();

    /// <inheritdoc />
    public Task<T> RunAsync<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        _threadQueue.Add(() =>
            {
                try { tcs.SetResult(func()); }
                catch (Exception ex) { tcs.SetException(ex); }
            }
        );
        return tcs.Task;
    }

    #endregion
}