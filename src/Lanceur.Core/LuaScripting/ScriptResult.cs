namespace Lanceur.Core.LuaScripting
{
    public class ScriptResult : Script
    {
        #region Properties

        public string Error { get; init; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $"File Name  : {Context.FileName}" +
                 $"\nParameters : {Context.Parameters}";
        }

        #endregion Methods
    }
}