using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.ViewModels.Controls;

/// <summary>
///     Contains the description of a chart along with the data bind to it
/// </summary>
/// <param name="Descriptor"></param>
/// <param name="Points"></param>
public sealed record ChartDataViewModel(ChartDescriptorViewModel Descriptor, IReadOnlyCollection<ChartPoint> Points);