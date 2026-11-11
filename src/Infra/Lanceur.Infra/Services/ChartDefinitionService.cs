using System.Globalization;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public class ChartDefinitionService : IChartDefinitionService
{
    #region Fields

    private readonly IChartDefinitionRepository _chartDefinitionRepository;

    #endregion

    #region Constructors

    public ChartDefinitionService(IChartDefinitionRepository chartDefinitionRepository) =>
        _chartDefinitionRepository = chartDefinitionRepository;

    #endregion

    #region Methods

    private IReadOnlyCollection<ChartFilter> FetchChartFilterDefinitions(IEnumerable<ChartFilterDefinition> filters)
    {
        var r = filters.Select(NewFilter).ToList();
        return r;

        ChartFilter NewFilter(ChartFilterDefinition filter) =>
            new()
            {
                Title = filter.Title,
                Options = _chartDefinitionRepository.FetchFilter(filter),
                Parameter = filter.ParamName
            };
    }

    public IReadOnlyCollection<ChartDescriptor> FetchChartDefinitions()
    {
        var definitions = _chartDefinitionRepository.FetchChartDefinitions();
        if (definitions.Count == 0) { return []; }

        var charts = new List<ChartDescriptor>();
        foreach (var definition in definitions)
        {
            charts.Add(new ChartDescriptor
            {
                Id = definition.Id,
                Title = definition.Title,
                XLabel = definition.XLabel,
                YLabel = definition.YLabel,
                XAxisKind = ToKind(definition),
                Filters = FetchChartFilterDefinitions(definition.Filters)
            });
        }

        return charts;

        static AxisKind ToKind(ChartDefinition definition)
            => definition.XKind.ToLower(CultureInfo.InvariantCulture) switch
            {
                "numeric" => AxisKind.Numeric,
                "date"    => AxisKind.Date,
                "text"    => AxisKind.Text,
                _         => throw new ArgumentOutOfRangeException(nameof(definition), definition, null)
            };
    }

    public IReadOnlyCollection<ChartPoint> FetchChartPoints(Guid chartId, Dictionary<string, object>? filters = null)
        => _chartDefinitionRepository.FetchDataPoints(chartId, filters ?? []);

    #endregion
}