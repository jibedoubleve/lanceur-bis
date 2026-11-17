using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.Extensions;

namespace Lanceur.Ui.Core.ViewModels.Controls;

[ExcludeFromIoC]
public partial class ChartDescriptorViewModel : ObservableObject
{
    #region Constructors

    public ChartDescriptorViewModel(ChartDescriptor chart)
    {
        Id = chart.Id;
        Title = chart.Title;
        ChartType = chart.ChartType;
        SeriesLabel = chart.SeriesLabel;
        XLabel = chart.XLabel;
        YLabel = chart.YLabel;
        Filters = chart.Filters.ToViewModels();
        XAxisKind = chart.XAxisKind;
    }

    #endregion

    #region Properties

    public ChartType ChartType { get; }
    public IReadOnlyCollection<ChartFilterViewModel> Filters { get; }
    public Guid Id { get; }

    [ObservableProperty]
    public partial ChartFilterViewModel? SelectedFilter { get; set; }

    public string? SeriesLabel { get; }
    public string Title { get; }
    public AxisKind XAxisKind { get; }
    public string? XLabel { get; }
    public string? YLabel { get; }

    #endregion
}