using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Scripting;

namespace Lanceur.Infra.Scripting;

public class ScriptEngineFactory : IScriptEngineFactory
{
    #region Fields

    private readonly IEnumerable<IScriptEngine> _engines;
    private readonly ISection<ScriptingSection> _settings;

    #endregion

    #region Constructors

    public ScriptEngineFactory(IEnumerable<IScriptEngine> engines, ISection<ScriptingSection> settings)
    {
        _engines = engines;
        _settings = settings;
    }

    #endregion

    #region Properties

    public IScriptEngine Current
    {
        get
        {
            _settings.Reload();
            return _engines.FirstOrDefault(e => e.Language == _settings.Value.ScriptLanguage);
        }
    }

    #endregion
}