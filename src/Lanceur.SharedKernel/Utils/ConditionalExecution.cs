using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

/// <summary>
/// Provides methods for executing different logic depending on the compilation mode (DEBUG or RELEASE).
/// </summary>
public static class ConditionalExecution
{
    #region Methods

    [Conditional("DEBUG")] private static void SetIfDebug(ref bool isDebug) => isDebug = true;

    /// <summary>
    ///     Executes one of the provided functions based on the current compilation mode.
    /// </summary>
    /// <typeparam name="TContext">The type of the context passed to the functions.</typeparam>
    /// <param name="serviceCollection">The context object passed to the functions.</param>
    /// <param name="onDebug">The function to execute in DEBUG mode.</param>
    /// <param name="onRelease">The function to execute in RELEASE mode.</param>
    public static void Execute<TContext>(TContext serviceCollection, Action<TContext> onDebug, Action<TContext> onRelease)
    {
        var isDebug = false;
        SetIfDebug(ref isDebug);

        if(isDebug) onDebug(serviceCollection);
        else onRelease(serviceCollection);
    }
    
    /// <summary>
    ///     Executes one of the provided functions based on the current compilation mode.
    /// </summary>
    /// <param name="onDebug">The function to execute in DEBUG mode.</param>
    /// <param name="onRelease">The function to execute in RELEASE mode.</param>
    public static void Execute(Action onDebug, Action onRelease)
    {
        var isDebug = false;
        SetIfDebug(ref isDebug);

        if (isDebug) onDebug?.Invoke();
        else onRelease?.Invoke();
    }
    
    /// <summary>
    /// Provides an inverted argument version of <see cref="Execute"/> to facilitate debugging.
    /// This method swaps the logic, executing <paramref name="onDebug"/> in place of <paramref name="onRelease"/> and vice versa.
    /// </summary>
    /// <param name="onRelease">The action to execute in release mode.</param>
    /// <param name="onDebug">The action to execute in debug mode.</param>
    public static void ExecuteFlipped(Action onRelease, Action onDebug) => Execute(onDebug, onRelease);
    
    /// <summary>
    /// Provides an inverted argument version of <see cref="Execute"/> to facilitate debugging.
    /// This method swaps the logic, executing <paramref name="onDebug"/> in place of <paramref name="onRelease"/> and vice versa.
    /// </summary>
    /// <param name="onRelease">The action to execute in release mode.</param>
    /// <param name="onDebug">The action to execute in debug mode.</param>
    /// /// <returns>The result of the executed function.</returns>
    public static TResult ExecuteFlipped<TResult>(Func<TResult> onRelease, Func<TResult> onDebug) => Execute(onDebug, onRelease);


    /// <summary>
    ///     Executes one of the provided functions based on the current compilation mode and returns a value.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the functions.</typeparam>
    /// <param name="onDebug">The function to execute in DEBUG mode.</param>
    /// <param name="onRelease">The function to execute in RELEASE mode.</param>
    /// <returns>The result of the executed function.</returns>
    public static TReturn Execute<TReturn>(Func<TReturn> onDebug, Func<TReturn> onRelease)
    {
        var isDebug = false;
        SetIfDebug(ref isDebug);

        return isDebug
            ? onDebug()
            : onRelease();
    }

    #endregion
}