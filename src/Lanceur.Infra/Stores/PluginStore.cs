using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Plugins;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Splat;
using System.Reflection;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Stores;

[Store]
public class PluginStore : ISearchService, IPluginManifestRepository
{
    #region Fields

    private static IEnumerable<SelfExecutableQueryResult> _plugins;
    private readonly Version _appVersion;
    private readonly IPluginStoreContext _context;
    private readonly IDbRepository _dbRepository;
    private readonly ILogger<PluginStore> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPluginManager _pluginManager;

    #endregion Fields

    #region Constructors

    public PluginStore(
        IPluginStoreContext context = null,
        IPluginManager pluginManager = null,
        IDbRepository dbRepository = null,
        ILoggerFactory loggerFactory = null
    )
    {
        var l = Locator.Current;

        _loggerFactory = loggerFactory ?? l.GetService<ILoggerFactory>();
        _logger = LoggerFactoryMixin.GetLogger<PluginStore>(_loggerFactory);

        _dbRepository = dbRepository ?? l.GetService<IDbRepository>();
        _pluginManager = pluginManager ?? l.GetService<IPluginManager>();
        _context = context ?? l.GetService<IPluginStoreContext>();
        _appVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }

    public PluginStore() : this(null) { }

    #endregion Constructors

    #region Methods

    private void LoadPlugins()
    {
        if (_plugins != null) return;

        var configs = GetPluginManifests();
        _plugins = configs
                   .Where(manifest => _appVersion >= manifest.AppMinVersion)
                   .SelectMany(manifest => _pluginManager.CreatePlugin(manifest.Dll))
                   .Select(x => new PluginExecutableQueryResult(x, _loggerFactory))
                   .ToList();
    }

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.SharedAlwaysActive();

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll()
    {
        LoadPlugins();
        return _plugins;
    }

    public IPluginManifest[] GetPluginManifests()
    {
        var root = _context.RepositoryPath;
        var files = Directory.EnumerateFiles(root, Locations.ManifestFileName, SearchOption.AllDirectories);

        return files.Select(
                        file =>
                        {
                            var json = File.ReadAllText(file);
                            return (IPluginManifest)JsonConvert.DeserializeObject<PluginManifest>(json);
                        }
                    )
                    .ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        LoadPlugins();
        var found = (from plugin in _plugins
                     where plugin?.Name?.ToLower().StartsWith(query.Name.ToLower()) ?? false
                     select plugin).ToArray();
        _logger.LogTrace("Found {Length} plugin(s)", found.Length);

        //Set count and name
        foreach (var item in found) _dbRepository.Hydrate(item);
        return found;
    }

    #endregion Methods
}