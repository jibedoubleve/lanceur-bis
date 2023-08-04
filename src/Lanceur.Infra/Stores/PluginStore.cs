using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Plugins;
using Lanceur.Infra.Utils;
using Newtonsoft.Json;
using Splat;
using System.Reflection;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class PluginStore : ISearchService, IPluginManifestRepository
    {
        #region Fields

        private static IEnumerable<SelfExecutableQueryResult> _plugins = null;
        private readonly Version _appVersion;
        private readonly IPluginStoreContext _context;
        private readonly IDbRepository _dbRepository;
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
            IPluginManager pluginManager = null,
            IDbRepository dbRepository = null
        )
        {
            var l = Locator.Current;
            _dbRepository = dbRepository ?? l.GetService<IDbRepository>();
            _pluginManager = pluginManager ?? l.GetService<IPluginManager>();
            _context = context ?? l.GetService<IPluginStoreContext>();

            _logFactory = logFactory ?? l.GetService<IAppLoggerFactory>();
            _log = l.GetLogger<PluginStore>(_logFactory);

            _appVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }

        #endregion Constructors

        #region Methods

        private void LoadPlugins()
        {
            if (_plugins == null)
            {
                var configs = GetPluginManifests();

                _plugins = configs
                    .Where(manifest => _appVersion >= manifest.AppMinVersion)
                    .SelectMany(manifest => _pluginManager.CreatePlugin(manifest.Dll))
                    .Select(x => new PluginExecutableQueryResult(x, _logFactory))
                    .ToList();
            }
        }

        public IEnumerable<QueryResult> GetAll()
        {
            LoadPlugins();
            return _plugins;
        }

        public IPluginManifest[] GetPluginManifests()
        {
            var root = _context.RepositoryPath;
            var files = Directory.EnumerateFiles(root, PluginLocation.ManifestName, SearchOption.AllDirectories);

            return files.Select(file =>
            {
                var json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<PluginManifest>(json);
            }).ToArray();
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            LoadPlugins();
            var found = (from plugin in _plugins
                         where plugin?.Name?.ToLower()?.StartsWith(query.Name.ToLower()) ?? false
                         select plugin).ToArray();
            _log.Trace($"Found {found.Length} plugin(s)");
            return found;
        }

        #endregion Methods
    }
}