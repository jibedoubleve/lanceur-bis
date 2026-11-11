using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IChartDefinitionService
{
    #region Methods

    IReadOnlyCollection<ChartDescriptor> FetchChartDefinitions();

    IReadOnlyCollection<ChartPoint> FetchChartPoints(Guid chartId, Dictionary<string, object>? filters = null);

    #endregion
}