using System.Windows;
using Lanceur.Ui.Core.ViewModels.Controls;
using Lanceur.Ui.WPF.Views.Controls.Charts;
using Wpf.Ui.Appearance;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;

namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Displays a chart. The drawing itself is delegated to the renderer matching the type of the chart,
///     this control only handles the theme and the layout shared by every chart.
/// </summary>
public partial class ChartView
{
    #region Fields

    public static readonly DependencyProperty ChartProperty =
        DependencyProperty.Register(
            nameof(Chart),
            typeof(ChartDataViewModel),
            typeof(ChartView),
            new PropertyMetadata(null, (d, e) => ((ChartView)d).Draw(e.NewValue as ChartDataViewModel)));

    /// <summary>
    ///     Renderer of the chart currently displayed, kept to adapt its layout when the control is resized.
    /// </summary>
    private IChartRenderer? _renderer;

    #endregion

    #region Constructors

    public ChartView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Properties

    public ChartDataViewModel? Chart
    {
        get => (ChartDataViewModel?)GetValue(ChartProperty);
        set => SetValue(ChartProperty, value);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Applies the theme to the elements shared by every chart: background, axes, grid and legend.
    /// </summary>
    private void ApplyTheme(ChartTheme theme)
    {
        var plot = Plot.Plot;

        // Transparent: the page background (Mica / Fluent background) shows through
        plot.FigureBackground.Color = Colors.Transparent;
        plot.DataBackground.Color = Colors.Transparent;

        plot.Axes.Color(theme.SecondaryText);
        plot.Axes.Title.Label.ForeColor = theme.Text;
        plot.Axes.Bottom.Label.ForeColor = theme.Text;
        plot.Axes.Left.Label.ForeColor = theme.Text;
        plot.Axes.Bottom.FrameLineStyle.Color = theme.Axis;
        plot.Axes.Left.FrameLineStyle.Color = theme.Axis;
        plot.Axes.Top.FrameLineStyle.Color = Colors.Transparent;
        plot.Axes.Right.FrameLineStyle.Color = Colors.Transparent;

        plot.Grid.MajorLineColor = theme.Grid;

        plot.Legend.BackgroundColor = Colors.Transparent;
        plot.Legend.OutlineColor = Colors.Transparent;
        plot.Legend.FontColor = theme.Text;
    }

    private void Draw(ChartDataViewModel? chart)
    {
        // A new plot: a previous chart (ticks, hidden axes, colour bar, legend...) doesn't leak into the next one
        Plot.Reset();
        _renderer = null;

        var theme = LoadTheme();
        ApplyTheme(theme);

        if (chart is null || chart.Points.Count == 0)
        {
            Plot.Refresh();
            return;
        }

        var plot = Plot.Plot;
        plot.Title(chart.Descriptor.Title);
        plot.XLabel(chart.Descriptor.XLabel ?? string.Empty);
        plot.YLabel(chart.Descriptor.YLabel ?? string.Empty);

        _renderer = ChartRenderer.Create(chart.Descriptor.ChartType);
        _renderer.Render(plot, chart, theme);
        _renderer.Relayout(plot, Plot.ActualWidth);

        Plot.Refresh();
    }

    /// <summary>
    ///     Reads the Fluent theme brushes from the WPF-UI resources so the chart follows
    ///     the same palette as the rest of the application (light, dark and accent).
    /// </summary>
    private ChartTheme LoadTheme()
        => new(
            ThemeColor("TextFillColorPrimaryBrush", Colors.Black),
            ThemeColor("TextFillColorSecondaryBrush", Colors.Gray),
            ThemeColor("ControlStrongStrokeColorDefaultBrush", Colors.Gray),
            ThemeColor("DividerStrokeColorDefaultBrush", Colors.LightGray),
            ThemeColor("AccentFillColorDefaultBrush", Color.FromHex("#0078D4"))
        );

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Changed += OnThemeChanged;
        Draw(Chart);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_renderer is null || !e.WidthChanged) { return; }

        if (_renderer.Relayout(Plot.Plot, Plot.ActualWidth)) { Plot.Refresh(); }
    }

    private void OnThemeChanged(ApplicationTheme theme, System.Windows.Media.Color accent) => Draw(Chart);

    private void OnUnloaded(object sender, RoutedEventArgs e) => ApplicationThemeManager.Changed -= OnThemeChanged;

    private Color ThemeColor(string resourceKey, Color fallback)
        => TryFindResource(resourceKey) is SolidColorBrush brush
            ? new(brush.Color.R,
                brush.Color.G,
                brush.Color.B,
                brush.Color.A)
            : fallback;

    #endregion
}