using System.Globalization;
using System.Resources;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Lanceur.Infra.SQLite.Repositories;

public class SQLiteChartDefinitionRepository : SQLiteRepositoryBase, IChartDefinitionRepository
{
    #region Fields

    private static readonly string[] Resources = ScriptRepository.Asm.GetManifestResourceNames()
                                                                 .Where(d => d.StartsWith("Lanceur.Scripts.Charts"))
                                                                 .ToArray();

    private readonly ILogger<SQLiteChartDefinitionRepository> _logger;

    #endregion

    #region Constructors

    public SQLiteChartDefinitionRepository(
        IDbConnectionManager manager, ILogger<SQLiteChartDefinitionRepository> logger) : base(manager)
        => _logger = logger;

    #endregion

    #region Methods

    private static IReadOnlyCollection<ChartFilterDefinition> FetchFilterDefinitions(JObject chart)
    {
        if (chart["filters"] is not JArray filters) { return []; }

        var result = new List<ChartFilterDefinition>();
        foreach (var json in filters.OfType<JObject>())
        {
            var filter = json.ToObject<ChartFilterDefinition>();
            if (filter is null) { continue; }

            var sqlSource = json.Value<string>("sqlSource");
            result.Add(filter with { Sql = sqlSource is null ? string.Empty : FetchSql(sqlSource) });
        }

        return result;
    }

    private static string FetchSql(string sqlSource)
    {
        var rsc = Resources.FirstOrDefault(d => d.EndsWith(sqlSource));

        if (string.IsNullOrWhiteSpace(rsc))
        {
            throw new MissingManifestResourceException($"Missing resource: {sqlSource}");
        }

        using var stream = ScriptRepository.Asm.GetManifestResourceStream(rsc);
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }

    public IReadOnlyCollection<ChartDefinition> FetchChartDefinitions()
    {
        var def = new List<ChartDefinition>();

        var resources = Resources.Where(d => d.EndsWith(".json"));

        foreach (var res in resources)
        {
            using var stream = ScriptRepository.Asm.GetManifestResourceStream(res)!;
            using var reader = new StreamReader(stream);

            var json = JObject.Parse(reader.ReadToEnd());
            var sqlSource = json.Value<string>("sqlSource");
            var chart = json.ToObject<ChartDefinition>();

            if (chart is null) { continue; }

            if (sqlSource is null) { continue; }

            def.Add(chart with { Sql = FetchSql(sqlSource), Filters = FetchFilterDefinitions(json) });
        }

        return [.. def.OrderBy(d => d.Order)];
    }

    public IReadOnlyCollection<ChartPoint> FetchDataPoints(Guid descriptorId, Dictionary<string, object> parameters)
    {
        var descriptors = FetchChartDefinitions();
        var descriptor = descriptors.Single(d => d.Id == descriptorId);

        return Db.WithConnection(c => {
            var raw = c.Query<RawPoint>(descriptor.Sql, parameters)
                       .ToList();

            return raw.Select((p, i) => new ChartPoint
            {
                X = ToKind(p.X, descriptor.XKind, i),
                Y = (long)p.Y,
                Label = ToLabel(p.X, descriptor.XKind)
            }).ToList();
        });

        static string? ToLabel(object value, string xKind) 
            => xKind.ToLowerInvariant() == "text" ? value.ToString() : null;

        static double ToKind(object value, string xKind, int fallback)
            => xKind.ToLowerInvariant() switch
            {
                "date"    => DateTime.Parse(value.ToString() ?? string.Empty, CultureInfo.InvariantCulture).ToOADate(),
                "numeric" => Convert.ToDouble(value, CultureInfo.InvariantCulture),
                "text"    => fallback,
                _         => throw new NotSupportedException($"Unknown kind: {xKind}")
            };
    }

    public IDictionary<string, object?> FetchFilter(ChartFilterDefinition filter)
    {
        if (filter.Sql.IsNullOrWhiteSpace())
        {
            _logger.LogWarning("No SQL definition for filter {Filter}", filter.Title);
            return new Dictionary<string, object?>();
        }

        return Db.WithConnection(c => c.Query<RawFilter>(filter.Sql)
                                       .ToDictionary(k => k.Key, v => v.Value));
    }

    #endregion

    private sealed class RawPoint
    {
        #region Properties

        public required object X { get; init; }
        public required object Y { get; init; }

        #endregion
    }

    private sealed class RawFilter
    {
        #region Properties

        public required string Key { get; init; }
        public required object? Value { get; init; }

        #endregion
    }
}