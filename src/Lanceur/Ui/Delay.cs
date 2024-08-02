using System;
using System.Threading.Tasks;

namespace Lanceur.Ui;

/// <summary>
/// Propose an editable implementation of <code>Task.Delay</code>.
/// </summary>
public interface IDelay
{
    #region Methods

    /// <summary>
    /// Creates a Task that will complete after a time delay.
    /// </summary>
    /// <param name="millisecondsDelay">Length of the delay.</param>
    Task Of(int millisecondsDelay);

    /// <summary>
    /// Creates a Task that will complete after a time delay.
    /// </summary>
    /// <param name="delay">Length of the delay.</param>
    Task Of(TimeSpan delay);

    #endregion Methods
}

/// <summary>
/// Default implementation of a <see cref="IDelay"/>. which is a <code>Task.Delay()</code>
/// </summary>
public class Delay : IDelay
{
    #region Methods

    public async Task Of(int millisecondsDelay) => await Task.Delay(millisecondsDelay);

    public async Task Of(TimeSpan delay) => await Task.Delay(delay);

    #endregion Methods
}

/// <summary>
/// Dummy implementation of a <see cref="IDelay"/> which does nothing but
/// returning a <code>Task.Completed</code>
/// </summary>
public class DummyDelay : IDelay
{
    #region Methods

    public Task Of(int millisecondsDelay) => Task.CompletedTask;

    public Task Of(TimeSpan delay) => Task.CompletedTask;

    #endregion Methods
}