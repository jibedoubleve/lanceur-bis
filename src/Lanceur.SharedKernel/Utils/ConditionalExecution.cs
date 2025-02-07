using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public static class ConditionalExecution
{
    #region Methods

    private static void CheckIfDebug(ref bool isDebug) => isDebug = true;

    [Conditional("DEBUG")]
    private static void ExecuteIfDebug<TContext>(ref bool isDebug, TContext context, Func<TContext, object> onDebug)
    {
        isDebug = true;
        onDebug?.Invoke(context);
    }

    /// <summary>
    ///     Executes one of the provided functions based on the current compilation mode.
    /// </summary>
    /// <typeparam name="TContext">The type of the context passed to the functions.</typeparam>
    /// <param name="serviceCollection">The context object passed to the functions.</param>
    /// <param name="onDebug">The function to execute in DEBUG mode.</param>
    /// <param name="onRelease">The function to execute in RELEASE mode.</param>
    public static void Execute<TContext>(TContext serviceCollection, Func<TContext, object> onDebug, Func<TContext, object> onRelease)
    {
        var isDebug = false;

        ExecuteIfDebug(ref isDebug, serviceCollection, onDebug);
        if (!isDebug) onRelease(serviceCollection);
    }

    public static void Execute(Action onDebug, Action onRelease)
    {
        var isDebug = false;
        CheckIfDebug(ref isDebug);

        if (isDebug)
            onDebug();
        else
            onRelease();
    }

    /// <summary>
    ///     Executes one of the provided functions based on the current compilation mode and returns a value.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the functions.</typeparam>
    /// <param name="onDebug">The function to execute in DEBUG mode.</param>
    /// <param name="onRelease">The function to execute in RELEASE mode.</param>
    /// <returns>The result of the executed function.</returns>
    public static TReturn Return<TReturn>(Func<TReturn> onDebug, Func<TReturn> onRelease)
    {
        var isDebug = false;
        CheckIfDebug(ref isDebug);

        return isDebug
            ? onDebug()
            : onRelease();
    }

    #endregion
}