namespace Lanceur.Core.Scripting;

public interface IScriptEngineFactory
{
    IScriptEngine Current { get; }
}