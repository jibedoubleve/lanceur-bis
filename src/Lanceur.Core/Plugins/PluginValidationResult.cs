namespace Lanceur.Core.Plugins;

public class PluginValidationResult
{
    #region Constructors

    private PluginValidationResult(bool isValid, bool isUpdate, string message)
    {
        IsValid = isValid;
        Message = message;
        IsUpdate = isUpdate;
    }

    #endregion Constructors

    #region Properties

    public bool IsInvalid => !IsValid;

    public bool IsValid { get; }

    public bool IsUpdate { get; }

    public string Message { get; }

    #endregion Properties

    #region Methods

    public static PluginValidationResult BuildInvalid(string message) => new(false, false, message);

    public static PluginValidationResult BuildValid(bool isUpdate = false) => new(true, isUpdate, string.Empty);

    #endregion Methods
}