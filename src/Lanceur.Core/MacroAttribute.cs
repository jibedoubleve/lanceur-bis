namespace Lanceur.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MacroAttribute : Attribute
    {
        #region Fields

        private readonly string _name;

        #endregion Fields

        #region Constructors

        public MacroAttribute(string name)
        {
            _name = name.Trim().Replace("@", "").ToUpper();
        }

        #endregion Constructors

        #region Properties

        public string Name => $"@{_name}@";

        #endregion Properties
    }
}