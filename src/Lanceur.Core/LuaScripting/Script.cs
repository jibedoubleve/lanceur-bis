namespace Lanceur.Core.LuaScripting;

public record Script
{
    #region Properties

    public string? Code { get; init; } = string.Empty;

    public ScriptContext Context { get; init; } = ScriptContext.Empty;

    #endregion
}