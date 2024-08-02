namespace Lanceur.Core.Plugins;

public class PluginInstallationResult
{
    #region Constructors

    private PluginInstallationResult(
        IPluginManifest pluginManifest,
        bool isInstallationSuccess,
        string errorMessage,
        bool isUpdate
    )
    {
        ErrorMessage = errorMessage;
        IsUpdate = isUpdate;
        IsInstallationSuccess = isInstallationSuccess;
        PluginManifest = pluginManifest;
    }

    #endregion Constructors

    #region Properties

    public string ErrorMessage { get; }

    public bool IsInstallationSuccess { get; }
    public bool IsUpdate { get; }
    public IPluginManifest PluginManifest { get; }

    #endregion Properties

    #region Methods

    public static PluginInstallationResult Error(string errorMessage) => new(null, false, errorMessage, false);

    public static PluginInstallationResult Success(IPluginManifest manifest, bool isUpdate = false) => new(manifest, true, null, isUpdate);

    #endregion Methods
}