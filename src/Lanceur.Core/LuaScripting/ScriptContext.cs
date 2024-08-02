namespace Lanceur.Core.LuaScripting;

public class ScriptContext
{
    #region Properties

    public static ScriptContext Empty => new();
    public string FileName { get; init; } = string.Empty;
    public string Parameters { get; init; } = string.Empty;

    #endregion Properties
}