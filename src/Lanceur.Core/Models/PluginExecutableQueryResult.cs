using Lanceur.Core.Plugins;
using Lanceur.Core.Plugins.Models;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Core.Models;

public sealed class PluginExecutableQueryResult : SelfExecutableQueryResult
{
    #region Fields

    private readonly ILogger<PluginExecutableQueryResult> _logger;
    private readonly IPlugin _plugin;

    #endregion Fields

    #region Constructors

    public PluginExecutableQueryResult(IPlugin plugin, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PluginExecutableQueryResult>();
        _plugin = plugin;
        Name = plugin.Name;
        Icon = plugin.Icon;
    }

    #endregion Constructors

    #region Properties

    public override string Description => _plugin?.Description ?? string.Empty;

    #endregion Properties

    #region Methods

    private static IEnumerable<QueryResult> ToQueryResult(IEnumerable<ResultItem> collection)
    {
        var results = new List<QueryResult>();
        foreach (var item in collection)
        {
            var current = new DisplayQueryResult(item.Name, item.Description, item.Icon);
            results.Add(current);
        }

        return results;
    }

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        if (cmdline == null || cmdline.Name.IsNullOrWhiteSpace())
        {
            _logger.LogInformation("Cannot execute plugin {Name}: the cmdline is empty", Name);
            return NoResult;
        }

        return _plugin is not null
            ? ToQueryResult(await _plugin.ExecuteAsync(cmdline.Parameters))
            : new List<QueryResult>();
    }

    #endregion Methods
}