using Lanceur.Core.Configuration.Sections;

namespace Lanceur.Core.Scripting;

public interface IScriptEngine
{
    #region Methods

    Task<ScriptResult> ExecuteScriptAsync(Script script);

    ScriptLanguage Language { get; }
    #endregion
}