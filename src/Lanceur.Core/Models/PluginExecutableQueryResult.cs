using Lanceur.Core.Plugins;
using Lanceur.Core.Plugins.Models;

namespace Lanceur.Core.Models
{
    public class PluginExecutableQueryResult : SelfExecutableQueryResult
    {
        #region Fields

        private readonly IPlugin _plugin;

        #endregion Fields

        #region Constructors

        public PluginExecutableQueryResult(IPlugin plugin)
        {
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
            return _plugin is not null
                ? ToQueryResult(await _plugin.ExecuteAsync(cmdline.Parameters))
                : new List<QueryResult>();
        }

        #endregion Methods
    }
}