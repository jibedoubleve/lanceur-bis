namespace Lanceur.Infra.Win32.Helpers;

/// <summary>
///     Represents a runner that executes delegates on a dedicated STA (Single-Threaded Apartment) thread.
///     Required for COM interop APIs — such as <c>IShellItemImageFactory</c> — that are not thread-safe
///     and must be called from an STA thread.
/// </summary>
public interface IStaThreadRunner : IDisposable
{
    #region Methods

    /// <summary>
    ///     Queues a function for execution on the dedicated STA thread and returns a task
    ///     that completes with the function's result.
    /// </summary>
    /// <param name="func">The function to execute on the STA thread.</param>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <returns>A task that resolves to the function's return value, or faults if the function throws.</returns>
    Task<T> RunAsync<T>(Func<T> func);

    #endregion
}