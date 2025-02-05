using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Logging;

public static class LoggerExtensions
{
    #region Methods

    /// <summary>
    /// Begins a logging scope with a single key-value pair added to the scope context.
    /// </summary>
    /// <param name="logger">
    /// The <see cref="ILogger"/> instance used to log messages within this scope.
    /// </param>
    /// <param name="key">
    /// The key for the context value being added to the logging scope. If the key does not start with '@', 
    /// an '@' symbol will be prefixed to the key automatically.
    /// </param>
    /// <param name="value">
    /// The value associated with the key that will be included in the scope context.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> instance representing the logging scope. 
    /// Disposing of this instance will end the scope.
    /// </returns>
    /// <remarks>
    /// This method allows the creation of a scoped logging context with a single key-value pair. 
    /// Scopes are useful for associating context information (such as request IDs or user IDs) with log messages. 
    /// The context is added to all log messages emitted within the scope.
    /// </remarks>
    public static IDisposable BeginSingleScope(this ILogger logger, string key, object value)
    {
        if (!key.StartsWith('@')) key = $"@{key}";
        return new LogScope(logger).Add(key, value)
                                   .BeginScope();
    }

    /// <summary>
    /// Starts a new logging scope that includes a unique correlation ID.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance to create the logging scope for.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> instance that, when disposed, will end the logging scope.
    /// </returns>
    /// <remarks>
    /// This method creates a new log scope that includes a randomly generated correlation ID (using <see cref="Guid.NewGuid"/>). 
    /// The correlation ID is used to track the flow of a request or operation across multiple components in a system.
    /// This is particularly useful for tracing and debugging purposes, as the correlation ID can be used to associate log messages
    /// from different layers of an application that are related to the same logical operation.
    ///
    /// <example>
    /// Here is an example of how to use the <see cref="BeginCorrelatedLogs"/> method:
    /// <code>
    /// using (logger.BeginCorrelationIdScope())
    /// {
    ///     // Perform operations that should be correlated
    ///     logger.LogInformation("Starting operation...");
    /// }
    /// </code>
    /// When the scope is disposed, the correlation ID will no longer be included in subsequent log messages.
    /// </example>
    /// </remarks>
    public static IDisposable BeginCorrelatedLogs(this ILogger logger)
    {
        return new LogScope(logger).Add("correlation-id", Guid.NewGuid().ToString())
                                   .BeginScope();
    }

    [Obsolete("Will be removed...")]
    public static void LogActivate<TView>(this ILogger logger) { logger.LogTrace("Activating view {View}", typeof(TView)); }

    #endregion Methods
}