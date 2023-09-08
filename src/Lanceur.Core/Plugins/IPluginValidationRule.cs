namespace Lanceur.Core.Plugins;

public interface IPluginValidationRule<out TValidationResult, in TInput>
{
    #region Methods

    TValidationResult Check(TInput input);

    #endregion Methods
}