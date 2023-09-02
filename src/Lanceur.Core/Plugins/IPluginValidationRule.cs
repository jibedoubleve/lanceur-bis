namespace Lanceur.Core.Plugins;

public interface IPluginValidationRule
{
    #region Methods

    PluginValidationResult Check(PluginManifest manifest);

    #endregion Methods
}