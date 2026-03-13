using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Logging;

public static class TaskErrorExtension
{
    #region Methods

    public static Task LogOnFaulted(
        this Task task,
        ILogger logger,
        string message,
        params object?[] args)
        => task.ContinueWith(
            context => logger.LogWarning(context.Exception, message, args),
            TaskContinuationOptions.OnlyOnFaulted
        );

    #endregion
}