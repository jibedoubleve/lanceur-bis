namespace Lanceur.SharedKernel.Utils;

/// <summary>
///     Runs an action when the scope opens and another one when it is disposed.
/// </summary>
/// <example>
///     <code>
/// using (BooleanScope.Create(() => _flag = false, () => _flag = true)) { /* _flag is false here */ }
/// </code>
/// </example>
public sealed class BooleanScope : IDisposable
{
    #region Fields

    private readonly Action _onDispose;

    #endregion

    #region Constructors

    private BooleanScope(Action onDispose) => _onDispose = onDispose;

    #endregion

    #region Methods

    /// <summary>
    ///     Runs <paramref name="onCreate" /> immediately and returns a scope that runs
    ///     <paramref name="onDispose" /> when disposed.
    /// </summary>
    public static BooleanScope Create(Action onCreate, Action onDispose)
    {
        onCreate();
        return new BooleanScope(onDispose);
    }

    /// <summary>
    ///     Ends the scope by running the dispose action.
    /// </summary>
    public void Dispose() => _onDispose();

    #endregion
}