namespace Lanceur.Core.LuaScripting;

public class ScriptResult : Script
{
    #region Properties

    public Exception Exception { get; init; }

    #endregion Properties

    #region Methods

    public override string ToString() => $"""
                                         File Name  : {Context.FileName}
                                         Parameters : {Context.Parameters}
                                         """;

    #endregion Methods
}