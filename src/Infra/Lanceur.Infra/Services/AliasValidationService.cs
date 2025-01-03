using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Services.Validators;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Services;

internal static class AliasValidationRulesExtensions
{
    #region Methods

    public static ValidationStatus ShouldNotBeNullOrEmpty(this string value, string error) => value.IsNullOrWhiteSpace() ? ValidationStatus.Invalid(error) : ValidationStatus.Valid();

    public static ValidationStatus ValidateString(this object str, Func<string, ValidationStatus> validate)
    {
        if (str is null) return ValidationStatus.Invalid("The value cannot be null. Please provide a valid input.");
        if (str is not string s) return ValidationStatus.Invalid("The input must be a valid string. Please enter text.");

        return validate(s);
    }

    #endregion
}

public class AliasValidationService : IAliasValidationService
{
    #region Fields

    private readonly IAliasRepository _repository;

    #endregion

    #region Constructors

    public AliasValidationService(IAliasRepository repository) => _repository = repository;

    #endregion

    #region Methods

    public ValidationStatus AreNamesUnique(object names, long idAlias)
    {
        if (names is null) return ValidationStatus.Invalid("The value cannot be null. Please provide a valid input.");
        if (names is not string s) return ValidationStatus.Invalid("The input must be a valid string. Please enter text.");

        var nameArray = s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(e => e.Trim())
                         .ToArray();

        if (!nameArray.Any()) return ValidationStatus.Invalid("Names are required and cannot be empty.");

        var existing = _repository.GetExistingAliases(nameArray, idAlias)
                                  .ToArray();
        return existing.Any()
            ? ValidationStatus.Invalid($"These names are already in use for other aliases: '{string.Join(", ", existing)}'") 
            : ValidationStatus.Valid();
    }

    public ValidationStatus IsFileNameValid(object fileName) => fileName.ValidateString(f => f.ShouldNotBeNullOrEmpty("Filename is required and cannot be empty."));
    public ValidationStatus IsNameValid(object name) => name.ValidateString(n => n.ShouldNotBeNullOrEmpty("An alias must contain at least one name, or multiple names separated by commas."));

    public ValidationStatus IsValid(AliasQueryResult alias)
    {
        if (alias is null) return ValidationStatus.Invalid("The alias should not be null.");

        var rules = new List<ValidationStatus>
        {
            IsNameValid(alias!.Name),
            IsFileNameValid(alias!.FileName),
            AreNamesUnique(alias!.Synonyms, alias!.Id)
        };

        if (rules.All(x => x.IsSuccess)) return ValidationStatus.Valid();

        var errors = rules.Where(x => !x.ErrorContent.IsNullOrWhiteSpace())
                          .Select(x => x.ErrorContent);
        var strErrors = string.Join(Environment.NewLine, errors);
        return ValidationStatus.Invalid(strErrors);
    }

    #endregion
}