using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories;

/// <summary>
///     Provides access to the chart definitions, built from the JSON descriptors and
///     the SQL scripts they reference.
/// </summary>
public interface IChartDefinitionRepository
{
    #region Methods

    /// <summary>
    ///     Loads every chart definition resource and resolves the SQL script it references.
    /// </summary>
    IReadOnlyCollection<ChartDefinition> FetchChartDefinitions();

    /// <summary>
    ///     Runs the SQL of the given definition and converts each row into a <see cref="ChartPoint" />.
    ///     Dates on the X axis are converted into OLE Automation dates.
    /// </summary>
    /// <param name="descriptorId">
    ///     id of the chart definition, in the json is can be found under
    ///     the property <c>id</c>
    /// </param>
    /// <param name="parameters">Names parameters used in the query</param>
    IReadOnlyCollection<ChartPoint> FetchDataPoints(Guid descriptorId, Dictionary<string, object> parameters);

    IDictionary<string, object?> FetchFilter(ChartFilterDefinition filter);

    #endregion
}