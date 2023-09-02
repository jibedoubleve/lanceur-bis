using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;

namespace Lanceur.Infra.Plugins;

public class PluginValidationRule : IPluginValidationRule
{
    #region Fields

    private readonly IPluginManifestRepository _manifestRepository;

    #endregion Fields

    #region Constructors

    public PluginValidationRule(IPluginManifestRepository manifestRepository)
    {
        _manifestRepository = manifestRepository;
    }

    #endregion Constructors

    #region Methods

    public PluginValidationResult Check(PluginManifest manifest)
    {
        var manifests = _manifestRepository.GetPluginManifests();

        // If there's no plugin installed, of course this
        // one can be installed...
        if (manifests.Length == 0)
        {
            return PluginValidationResult.BuildValid();
        }

        var isValid =
            (from m in manifests
             where manifest.Dll == m.Dll
                && manifest.Version > m.Version
             select m).Any();

        if (!isValid)
        {
            return PluginValidationResult.BuildInvalid(
                $"Cannot install plugin '{manifest.Name} V{manifest.Version}' because " +
                $"the installed version is already up to date.");
        }

        return PluginValidationResult.BuildValid();
    }

    #endregion Methods
}