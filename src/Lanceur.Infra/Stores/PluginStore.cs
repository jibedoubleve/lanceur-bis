using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Plugins;
using Lanceur.Infra.Utils;
using Newtonsoft.Json;
using Splat;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class PluginStore : ISearchService, IPluginConfigRepository
    {
        #region Fields

        private static IEnumerable<SelfExecutableQueryResult> _plugins = null;
        private readonly IPluginStoreContext _context;
        private readonly IAppLogger _log;
        private readonly IAppLoggerFactory _logFactory;
        private readonly IPluginManager _pluginManager;

        #endregion Fields

        #region Constructors

        public PluginStore() : this(null)
        {
        }

        public PluginStore(
            IPluginStoreContext context = null,
            IAppLoggerFactory logFactory = null,
            IPluginManager pluginManager = null
        )
        {
            var l = Locator.Current;
            _pluginManager = pluginManager ?? l.GetService<IPluginManager>();
            _context = context ?? l.GetService<IPluginStoreContext>();

            _logFactory = logFactory ?? l.GetService<IAppLoggerFactory>();
            _log = l.GetLogger<PluginStore>(_logFactory);
        }

        #endregion Constructors

        #region Methods

        public PluginConfiguration[] GetPluginConfigurations()
        {
            var configs = new List<PluginConfiguration>();
            var root = _context.RepositoryPath;
            var files = Directory.EnumerateFiles(root, "plugin.config.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var cfg = JsonConvert.DeserializeObject<PluginConfiguration>(json);
                configs.Add(cfg);
            }

            return configs.ToArray();
        }

        private void LoadPlugins()
        {
            if (_plugins == null)
            {
                var configs = GetPluginConfigurations();
                var queryResults = new List<PluginExecutableQueryResult>();

                foreach (var config in configs)
                {
                    var asm = _pluginManager.LoadPluginAsm($"plugins/{config.Dll}");
                    var plugins = _pluginManager.CreatePlugin(asm);
                    foreach (var plugin in plugins)
                    {
                        var query = new PluginExecutableQueryResult(plugin, _logFactory);
                        queryResults.Add(query);
                    }
                }
                _plugins = queryResults;
            }
        }

        public IEnumerable<QueryResult> GetAll()
        {
            LoadPlugins();
            return _plugins;
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            LoadPlugins();
            var found = from plugin in _plugins
                        where plugin?.Name?.ToLower()?.StartsWith(query.Name.ToLower()) ?? false
                        select plugin;
            _log.Trace($"Found {found.Count()} plugin(s)");
            return found;
        }

        #endregion Methods
    }
}