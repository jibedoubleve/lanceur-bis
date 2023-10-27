namespace Lanceur.Core.LuaScripting
{
    public class ScriptResult : Script
    {
        #region Properties

        public string Error { get; init; }
        public override string ToString()
        {
            return $"File Name  : {Context.FileName}" +
                 $"\nParameters : {Context.Parameters}";
        }

        #endregion Properties
    }
}