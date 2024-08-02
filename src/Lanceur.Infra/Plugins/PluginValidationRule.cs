using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;

namespace Lanceur.Infra.Plugins;

public class PluginValidationRule : IPluginValidationRule<PluginValidationResult, PluginManifest>
{
    #region Fields

    private readonly IPluginManifestRepository _manifestRepository;

    #endregion Fields

    #region Constructors

    public PluginValidationRule(IPluginManifestRepository manifestRepository) => _manifestRepository = manifestRepository;

    #endregion Constructors

    #region Methods

    public PluginValidationResult Check(PluginManifest manifest)
    {
        var manifests = _manifestRepository.GetPluginManifests();

        // If there's no plugin installed, of course this
        // one can be installed...
        if (manifests.Length == 0) return PluginValidationResult.Valid();

        var installed =
            (from current in manifests
             where manifest.Dll == current.Dll
             select current).FirstOrDefault();

        if (installed is not null && installed.Version >= manifest.Version)
            return PluginValidationResult.Invalid(
                $"Cannot install plugin '{manifest.Name} V{manifest.Version}' because " +
                $"the installed version is already up to date."
            );

        var isUpdate = (installed?.Version ?? new Version()) < manifest.Version;
        return PluginValidationResult.Valid(isUpdate);
    }

    #endregion Methods
}