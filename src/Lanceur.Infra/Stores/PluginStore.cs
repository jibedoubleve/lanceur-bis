using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Utils;
using Splat;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class PluginStore : ISearchService
    {
        #region Fields

        private readonly IPluginStoreContext _context;
        private readonly ILogService _log;
        private readonly IPluginManager _pluginManager;
        private static IEnumerable<QueryResult> _plugins = null;

        #endregion Fields

        #region Constructors

        public PluginStore() : this(null)
        {
        }

        public PluginStore(
            IPluginStoreContext context = null,
            ILogService logService = null,
            IPluginManager pluginManager = null
        )
        {
            var l = Locator.Current;
            _pluginManager = pluginManager ?? l.GetService<IPluginManager>();
            _context = context ?? l.GetService<IPluginStoreContext>();
            _log = logService ?? l.GetService<ILogService>() ?? new TraceLogService();
        }

        #endregion Constructors

        #region Methods

        private void LoadPlugin()
        {
            if (_plugins == null)
            {
                var foundPlugins = new List<QueryResult>();
                var dlls = FileManager.FindWithExtension(_context.RepositoryPath, ".dll");

                foreach (var dll in dlls)
                {
                    _log.Trace($"Found DLL '{dll}'. Looking for plugins.");
                    if (_pluginManager.Exists(dll))
                    {
                        var plugins = _pluginManager.GetPluginTypes(dll);
                        foreach (var plugin in plugins)
                        {
                            var queryResult = _pluginManager.Activate(plugin);
                            if (queryResult is not null)
                            {
                                _log.Info($"Found plugin '{(queryResult?.Name ?? "N.A.")}'");
                                foundPlugins.Add(queryResult);
                            }
                            else { _log.Warning($"plugin '{plugin.Name}' is cannot be loaded as a plugin."); }
                        }
                    }
                }
                _plugins = foundPlugins ?? new List<QueryResult>();
            }
        }

        public IEnumerable<QueryResult> GetAll()
        {
            LoadPlugin();
            return _plugins;
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            LoadPlugin();
            var found = from plugin in _plugins
                        where plugin?.Name?.ToLower()?.Contains(query.Name.ToLower()) ?? false
                        select plugin;
            return found;
        }

        #endregion Methods
    }
}