namespace Lanceur.Ui
{
    public class StringDisplay
    {
        #region Constructors

        public StringDisplay(string value, string display)
        {
            Value = value;
            Display = display;
        }

        #endregion Constructors

        #region Properties

        public string Display { get; }
        public string Value { get; }

        #endregion Properties
    }
}