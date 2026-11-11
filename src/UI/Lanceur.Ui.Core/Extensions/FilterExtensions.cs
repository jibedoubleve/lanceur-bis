using Lanceur.Core.Models;
using Lanceur.Ui.Core.ViewModels.Controls;

namespace Lanceur.Ui.Core.Extensions;

public static class FilterExtensions
{
    #region Methods

    public static IReadOnlyCollection<ChartFilterViewModel> ToViewModels(this IEnumerable<ChartFilter> filters)
        => [.. filters.Select(f => new ChartFilterViewModel(f))];

    public static IReadOnlyCollection<ChartDescriptorViewModel> ToViewModels(
        this IEnumerable<ChartDescriptor> descriptors)
        => [.. descriptors.Select(d => new ChartDescriptorViewModel(d))];


    #endregion
}