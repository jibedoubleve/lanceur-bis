using System.Windows.Controls;
using Lanceur.Core.Services.Validators;

namespace Lanceur.Ui.WPF.Validations;

public static class ValidationAdapter
{
    public static ValidationResult ToValidationResult(this ValidationStatus validationStatus) => validationStatus.IsSuccess
        ? ValidationResult.ValidResult
        : new(false, validationStatus.ErrorContent);
}