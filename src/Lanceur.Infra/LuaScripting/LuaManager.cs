using Lanceur.Core.LuaScripting;
using NLua;
using System.Text;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.LuaScripting;

public static class LuaManager
{
    #region Methods

    public static ScriptResult ExecuteScript(Script script)
    {
        if (script.Code.IsNullOrWhiteSpace()) return new() { Code    = script.Code ?? string.Empty, Context = new() { FileName = script.Context?.FileName ?? string.Empty, Parameters = script.Context?.Parameters ?? string.Empty } };

        using var lua = new Lua();
        try
        {
            lua.State.Encoding = Encoding.UTF8;
            lua["context"] = script.Context;
            var result = lua.DoString(script.Code);

            if (!result.Any()) return script.ToScriptResult();
            if (result[0] is not ScriptContext output) return script.ToScriptResult();

            return new() { Code = script.Code, Context = new() { FileName = output.FileName, Parameters = output.Parameters } };
        }
        catch (Exception e) { return new() { Code = script.Code, Exception = e }; }
    }

    #endregion Methods
}