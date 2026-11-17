using ScottPlot;

namespace Lanceur.Ui.WPF.Views.Controls.Charts;

/// <summary>
///     Colours of the Fluent theme currently applied, used by the renderers to draw the charts.
/// </summary>
internal sealed record ChartTheme(Color Text, Color SecondaryText, Color Axis, Color Grid, Color Accent)
{
    #region Fields

    private static readonly IPalette Palette = new ScottPlot.Palettes.Category10();

    #endregion

    #region Methods

    /// <summary>
    ///     Colour of the category at the specified index: the accent colour first, then a categorical palette.
    /// </summary>
    public Color Category(int index) => index == 0 ? Accent : Palette.GetColor(index);

    #endregion
}
