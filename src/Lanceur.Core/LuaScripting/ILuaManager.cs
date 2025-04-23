namespace Lanceur.Core.LuaScripting;

public interface ILuaManager
{
    #region Methods

    ScriptResult ExecuteScript(Script script);

    #endregion
}