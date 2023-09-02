namespace Lanceur.Core.Plugins;

public class PluginValidationResult
{
    #region Constructors

    private PluginValidationResult(bool isValid, string message)
    {
        IsValid = isValid;
        Message = message;
    }

    #endregion Constructors

    #region Properties

    public bool IsInvalid => !IsValid;

    public bool IsValid { get; }

    public string Message { get; }

    #endregion Properties

    #region Methods

    public static PluginValidationResult BuildInvalid(string message) => new(false, message);

    public static PluginValidationResult BuildValid() => new(true, string.Empty);

    #endregion Methods
}