using System.Globalization;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Services;

namespace Lanceur.Ui.WPF.Validations;

public class AliasNameRule : ValidationRule
{
    #region Fields

    private readonly IAliasValidationService _validationService;

    #endregion

    #region Constructors

    public AliasNameRule() => _validationService = Ioc.Default.GetService<IAliasValidationService>()!;

    #endregion

    #region Methods

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
        => _validationService.IsNameValid(value)
                             .ToValidationResult();

    #endregion
}