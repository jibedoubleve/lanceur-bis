using Lanceur.Core.Models;

namespace Lanceur.Core.LuaScripting;

public static class ScriptMixins
{
    #region Methods

    public static Script Clone(this Script script, string scriptCode) => new() { Code = scriptCode, Context = script?.Context ?? new() };

    public static Script ToScript(this AliasQueryResult alias) => new() { Code = alias.LuaScript, Context = new() { FileName = alias.FileName, Parameters = alias.Parameters } };

    public static ScriptResult ToScriptResult(this Script src) => new() { Code = src?.Code ?? string.Empty, Context = src?.Context ?? new() };

    #endregion Methods
}

public class Script
{
    #region Properties

    public string Code { get; init; } = string.Empty;

    public ScriptContext Context { get; init; } = ScriptContext.Empty;

    #endregion Properties
}