namespace Lanceur.Core.Services.Validators;

public record ValidationStatus
{
    #region Constructors

    private ValidationStatus(bool isSuccess, string errorContent)
    {
        IsSuccess = isSuccess;
        ErrorContent = errorContent;
    }

    #endregion

    #region Properties

    public string ErrorContent { get; }
    public bool IsSuccess { get; }

    #endregion

    #region Methods

    public static ValidationStatus Invalid(string errorMessage) => new(false, errorMessage);
    public static ValidationStatus Valid() => new(true, null);

    #endregion
}