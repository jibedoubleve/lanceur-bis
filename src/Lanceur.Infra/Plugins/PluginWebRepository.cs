using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;

namespace Lanceur.Infra.Plugins;

public class PluginWebRepository : IPluginWebRepository
{
    #region Fields

    private readonly IPluginManifestRepository _localPluginManifestRepository;
    private readonly IPluginWebManifestLoader _webPluginManifestLoader;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Get an instance of <see cref="PluginWebRepository"/>
    /// </summary>
    /// <param name="localPluginManifestRepository">Repository to have the list of installed plugins</param>
    /// <param name="webPluginManifestLoader">Loader to retrieve all the plugin in the web repository</param>
    public PluginWebRepository(
        IPluginManifestRepository localPluginManifestRepository,
        IPluginWebManifestLoader webPluginManifestLoader
    )
    {
        _localPluginManifestRepository = localPluginManifestRepository;
        _webPluginManifestLoader = webPluginManifestLoader;
    }

    #endregion Constructors

    #region Methods

    /// <inheritdoc />
    public async Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync(IEnumerable<IPluginManifest> uninstallationCandidates = null)
    {
        var webPlugins = await _webPluginManifestLoader.LoadFromWebAsync();
        var installedPlugins = _localPluginManifestRepository.GetPluginManifests();
        uninstallationCandidates ??= Array.Empty<IPluginManifest>();

        var plugins = installedPlugins.Where(i => uninstallationCandidates.All(x => x.Dll != i.Dll));

        // If a plugin is already installed but web version is
        // newer, then show this new plugin
        return webPlugins.Where(p => false == plugins.Any(x => x.Dll == p.Dll && x.Version >= p.Version))
                         .ToArray();
    }

    #endregion Methods
}