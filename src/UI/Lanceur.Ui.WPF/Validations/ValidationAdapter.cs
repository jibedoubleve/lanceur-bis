using System.Windows.Controls;
using Lanceur.Core.Services.Validators;

namespace Lanceur.Ui.WPF.Validations;

public static class ValidationAdapter
{
    #region Methods

    public static ValidationResult ToValidationResult(this ValidationStatus validationStatus)
        => validationStatus.IsSuccess
            ? ValidationResult.ValidResult
            : new ValidationResult(false, validationStatus.ErrorContent);

    #endregion
}