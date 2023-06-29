namespace Lanceur.Core.Models
{
    public class Cmdline
    {
        #region Constructors

        public Cmdline(string name, string parameters = "")
        {
            Name = name ?? string.Empty;
            Parameters = parameters ?? string.Empty;
        }

        #endregion Constructors

        #region Properties

        public static Cmdline Empty => new(string.Empty, string.Empty);
        public string Name { get; }
        public string Parameters { get; }

        #endregion Properties

        #region Methods

        public override string ToString() => $"{Name ?? string.Empty} {Parameters ?? string.Empty}".Trim();

        #endregion Methods
    }
}