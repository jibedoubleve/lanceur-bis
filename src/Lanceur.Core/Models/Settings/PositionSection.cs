namespace Lanceur.Core.Models.Settings;

public class PositionSection
{
    #region Properties

    public static PositionSection Default => new() { Left = double.MaxValue, Top = double.MaxValue };

    public double Left { get; set; }
    public double Top { get; set; }

    #endregion Properties
}