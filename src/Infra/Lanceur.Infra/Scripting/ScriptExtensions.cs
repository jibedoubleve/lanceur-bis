using Lanceur.Core.Scripting;

namespace Lanceur.Infra.Scripting;

public static class ScriptExtensions
{
    extension(Script script)
    {
        #region Methods

        public ScriptResult ToScriptError(Exception ex) => new() { Code = script.Code ?? string.Empty, Exception = ex };

        public ScriptResult ToScriptResult(ScriptContext scriptContext)
            => new() { Code = script.Code ?? string.Empty, Context = scriptContext };

        public ScriptResult ToScriptResult() => new()
        {
            Code = script.Code ?? string.Empty,
            Context = new()
            {
                FileName = script.Context?.FileName ?? string.Empty,
                Parameters = script.Context?.Parameters ?? string.Empty
            }
        };

        #endregion
    }
}