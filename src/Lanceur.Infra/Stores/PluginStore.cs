﻿using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
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
    public class PluginStore : ISearchService, IPluginManifestRepository
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

        public IPluginManifest[] GetPluginManifests()
        {
            var configs = new List<PluginManifest>();
            var root = _context.RepositoryPath;
            var files = Directory.EnumerateFiles(root, "plugin.config.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var cfg = JsonConvert.DeserializeObject<PluginManifest>(json);
                configs.Add(cfg);
            }

            return configs.ToArray();
        }

        private void LoadPlugins()
        {
            if (_plugins == null)
            {
                var configs = GetPluginManifests();

                _plugins = configs
                    .SelectMany(x => _pluginManager.CreatePlugin($"plugins/{x.Dll}"))
                    .Select(x => new PluginExecutableQueryResult(x, _logFactory))
                    .ToList();
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