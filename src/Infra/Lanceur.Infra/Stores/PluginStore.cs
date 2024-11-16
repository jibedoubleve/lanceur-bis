﻿using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Plugins;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Stores;

[Store]
public class PluginStore : IStoreService, IPluginManifestRepository
{
    #region Fields

    private readonly Version _appVersion;
    private readonly IPluginStoreContext _context;
    private readonly IDbRepository _dbRepository;
    private readonly ILogger<PluginStore> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPluginManager _pluginManager;

    private static IEnumerable<SelfExecutableQueryResult> _plugins;

    #endregion

    #region Constructors

    public PluginStore(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService<ILogger<PluginStore>>();
        _loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        _dbRepository = serviceProvider.GetService<IDbRepository>();
        _pluginManager = serviceProvider.GetService<IPluginManager>();
        _context = serviceProvider.GetService<IPluginStoreContext>();
        _appVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public Orchestration Orchestration => Orchestration.SharedAlwaysActive();

    #endregion

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
        var found = _plugins.Where(plugin => plugin?.Name?.ToLower().StartsWith(query.Name.ToLower()) ?? false)
                            .ToArray();
        _logger.LogTrace("Found {Length} plugin(s)", found.Length);

        //Set count and name
        foreach (var item in found) _dbRepository.Hydrate(item);
        return found;
    }

    #endregion
}