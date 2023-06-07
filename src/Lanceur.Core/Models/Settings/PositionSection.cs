namespace Lanceur.Core.Models.Settings
{
    public class PositionSection
    {
        #region Properties

        public static PositionSection Default => new() { Left = 600, Top = 150 };

        public double Left { get; set; }
        public double Top { get; set; }

        #endregion Properties
    }
}