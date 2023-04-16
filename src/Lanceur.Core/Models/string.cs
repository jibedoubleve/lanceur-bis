namespace Lanceur.Core.Models
{
    public class @string
    {
        #region Constructors

        public @string(string name, string parameters = "")
        {
            Name = name ?? string.Empty;
            Parameters = parameters ?? string.Empty;
        }

        #endregion Constructors

        #region Properties

        public static @string Empty => new(string.Empty, string.Empty);
        public string Name { get; }
        public string Parameters { get; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $"{(Name ?? string.Empty)} {(Parameters ?? string.Empty)}".Trim();
        }

        #endregion Methods
    }
}