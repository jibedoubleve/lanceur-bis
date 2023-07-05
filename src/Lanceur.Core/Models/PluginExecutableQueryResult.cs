using Lanceur.Core.Plugins;
using Lanceur.Core.Plugins.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.Models
{
    public class PluginExecutableQueryResult : SelfExecutableQueryResult
    {
        #region Fields

        private readonly IAppLogger _log;
        private readonly IPlugin _plugin;

        #endregion Fields

        #region Constructors

        public PluginExecutableQueryResult(IPlugin plugin, IAppLoggerFactory logFactory)
        {
            _log = logFactory.GetLogger<PluginExecutableQueryResult>();
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
                _log.Info($"Cannot execute plugin '{Name}': the cmdline is empty.");
                return NoResult;
            }

            return _plugin is not null
                ? ToQueryResult(await _plugin.ExecuteAsync(cmdline.Parameters))
                : new List<QueryResult>();
        }

        #endregion Methods
    }
}