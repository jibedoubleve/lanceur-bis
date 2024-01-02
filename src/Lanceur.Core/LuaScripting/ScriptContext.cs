namespace Lanceur.Core.LuaScripting
{
    public class ScriptContext
    {
        #region Properties

        public static ScriptContext Empty => new();
        public string FileName { get; init; }
        public string Parameters { get; init; }

        #endregion Properties
    }
}