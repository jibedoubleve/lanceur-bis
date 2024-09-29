using System.Diagnostics;

namespace Lanceur.SharedKernel.Utils;

public static class ConditionalExecution
{
    #region Methods

    /// <summary>
    /// Executes a function based on the current compilation symbol.
    /// </summary>
    /// <param name="serviceCollection">The context or service collection used to invoke the functions.</param>
    /// <param name="onDebug">The function to execute if the <c>DEBUG</c> symbol is defined.</param>
    /// <param name="onRelease">The function to execute if the <c>DEBUG</c> symbol is not defined (i.e., in release mode).</param>
    public static void Set<TContext>(TContext serviceCollection, Func<TContext, object> onDebug, Func<TContext, object> onRelease)
    {
        var isDebug = false;

        ExecuteIfDebug(ref isDebug, serviceCollection, onDebug);
        if (!isDebug) onRelease(serviceCollection);
    }

    [Conditional("DEBUG")]
    private static void ExecuteIfDebug<TContext>(ref bool isDebug, TContext context, Func<TContext, object> onDebug)
    {
        isDebug = true;
        onDebug(context);
    }

    #endregion
}