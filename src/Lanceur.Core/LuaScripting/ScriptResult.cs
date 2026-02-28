namespace Lanceur.Core.LuaScripting;

public class ScriptResult : Script
{
    #region Properties

    /// <summary>
    ///     Exception raised during script execution. Null if execution succeeded.
    /// </summary>
    public Exception Exception { get; init; }

    /// <summary>
    ///     Indicates whether script execution was cancelled via the execution context.
    /// </summary>
    public bool IsCancelled => Context.IsCancelled;

    /// <summary>
    ///     Captured output from logging operations performed during script execution.
    /// </summary>
    public string OutputContent { get; init; }

    #endregion

    #region Methods

    public override string ToString()
        => $"""
            File Name  : {Context.FileName}
            Parameters : {Context.Parameters}
            """;

    #endregion
}