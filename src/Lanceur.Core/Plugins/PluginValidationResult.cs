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

    public bool IsUpdate { get; }
    public bool IsValid { get; }
    public string Message { get; }

    #endregion Properties

    #region Methods

    public static PluginValidationResult Invalid(string message) => new(false, false, message);

    public static PluginValidationResult Valid(bool isUpdate = false) => new(true, isUpdate, string.Empty);

    #endregion Methods
}