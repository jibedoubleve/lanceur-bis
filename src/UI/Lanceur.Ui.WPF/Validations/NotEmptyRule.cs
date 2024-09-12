using System.Globalization;
using System.Windows.Controls;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Ui.WPF.Validations;

public class NotEmptyRule : ValidationRule
{
    #region Methods

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not string item) return ValidationResult.ValidResult;

        return item.IsNullOrWhiteSpace()
            ? new(false, "This should have a value")
            : ValidationResult.ValidResult;
    }

    #endregion
}