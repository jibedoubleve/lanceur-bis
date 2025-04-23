namespace Lanceur.Core.LuaScripting;

public class ScriptResult : Script
{
    #region Properties

    public Exception Exception { get; init; }

    /// <summary>
    ///     Gets a value indicating whether the script has been cancelled.
    /// </summary>
    public bool IsCancelled => Context.IsCancelled;

    #endregion

    #region Methods

    public override string ToString() => $"""
                                          File Name  : {Context.FileName}
                                          Parameters : {Context.Parameters}
                                          """;

    #endregion
}