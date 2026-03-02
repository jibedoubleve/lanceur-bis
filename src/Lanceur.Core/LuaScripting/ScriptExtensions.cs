namespace Lanceur.Core.LuaScripting;

public static class ScriptExtensions
{
    #region Methods

    public static ScriptResult ToScriptResult(this Script src)
        => new() { Code = src?.Code ?? string.Empty, Context = src?.Context ?? new() };

    #endregion
}