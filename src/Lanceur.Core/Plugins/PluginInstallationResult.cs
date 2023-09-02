namespace Lanceur.Core.Plugins;

public class PluginInstallationResult
{
    #region Constructors

    private PluginInstallationResult(IPluginManifest pluginManifest, bool isInstallationSuccess, string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsInstallationInstallationSuccess = isInstallationSuccess;
        PluginPluginManifest = pluginManifest;
    }

    #endregion Constructors

    #region Properties

    public string ErrorMessage { get; }

    public bool IsInstallationInstallationSuccess { get; }

    public IPluginManifest PluginPluginManifest { get; }

    #endregion Properties

    #region Methods

    public static PluginInstallationResult Error(string errorMessage) => new(null, false, errorMessage);

    public static PluginInstallationResult Success(IPluginManifest manifest) => new(manifest, true, null);

    #endregion Methods
}