namespace Lanceur.Core.LuaScripting
{
    public class ScriptContext
    {
        #region Properties

        public static ScriptContext Empty => new();
        public string FileName { get; set; }
        public string Parameters { get; set; }

        #endregion Properties
    }
}