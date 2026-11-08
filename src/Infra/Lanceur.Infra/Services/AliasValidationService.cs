using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Services.Validators;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Services;

internal static class AliasValidationRulesExtensions
{
    #region Methods

    public static ValidationStatus ShouldNotBeNullOrEmpty(this string value, string error)
        => value.IsNullOrWhiteSpace() ? ValidationStatus.Invalid(error) : ValidationStatus.Valid();

    public static ValidationStatus ValidateString(this object str, Func<string, ValidationStatus> validate) =>
        str is string s
            ? validate(s)
            : ValidationStatus.Invalid("The input must be a valid string. Please enter text.");

    #endregion
}

public sealed class AliasValidationService : IAliasValidationService
{
    #region Fields

    private readonly IAliasRepository _repository;

    private readonly ISection<StoreSection> _storeSection;

    private readonly IStoreShortcutService _storeShortcutService;

    #endregion

    #region Constructors

    public AliasValidationService(
        IAliasRepository repository,
        IStoreShortcutService storeShortcutService,
        ISection<StoreSection> storeSection)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(storeShortcutService);
        ArgumentNullException.ThrowIfNull(storeSection);

        _repository = repository;
        _storeShortcutService = storeShortcutService;
        _storeSection = storeSection;
    }

    #endregion

    #region Methods

    private ValidationStatus AreNamesNotStoreShortcuts(string? names)
    {
        if (names is null) { return ValidationStatus.Valid(); }

        var shortcuts = _storeShortcutService.Resolve(_storeSection.Value)
                                             .Select(s => s.AliasOverride)
                                             .Where(CmdlineManager.IsSpecialCharacter)
                                             .ToHashSet();
        var conflicts = names.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(e => e.Trim())
                             .Where(shortcuts.Contains)
                             .ToArray();

        return conflicts.Length != 0
            ? ValidationStatus.Invalid(
                $"These names are already used as store shortcuts: {string.Join(", ", conflicts)}"
            )
            : ValidationStatus.Valid();
    }

    private ValidationStatus AreNamesUnique(object? names, long idAlias)
    {
        if (names is null)
        {
            return ValidationStatus.Invalid("The names cannot be null. Please provide a valid input.");
        }

        if (names is not string s)
        {
            return ValidationStatus.Invalid("The names must be a valid string. Please enter text.");
        }

        var nameArray = s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(e => e.Trim())
                         .ToArray();

        if (nameArray.Length == 0) { return ValidationStatus.Invalid("Names are required and cannot be empty."); }

        var existing = _repository.GetExistingAliases(nameArray, idAlias)
                                  .ToArray();
        return existing.Length != 0
            ? ValidationStatus.Invalid(
                $"These names are already in use for other aliases: {string.Join(", ", existing)}"
            )
            : ValidationStatus.Valid();
    }

    private ValidationStatus IsDeleted(string? names, long idAlias)
    {
        if (names is null) { return ValidationStatus.Valid(); }

        var nameArray = names.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(e => e.Trim())
                             .ToArray();

        if (nameArray.Length == 0) { return ValidationStatus.Valid(); }

        var existing = _repository.GetExistingDeletedAliases(nameArray, idAlias)
                                  .ToArray();
        return existing.Length != 0
            ? ValidationStatus.Invalid(
                $"These names belong to a deleted alias: {string.Join(", ", existing)}. To use these names, restore the alias."
            )
            : ValidationStatus.Valid();
    }

    private static ValidationStatus IsNameValid(object name)
        => name.ValidateString(n
            => n.ShouldNotBeNullOrEmpty(
                "An alias must contain at least one name, or multiple names separated by commas."
            )
        );

    /// <inheritdoc />
    public ValidationStatus IsFileNameValid(object? fileName)
        => fileName?.ValidateString(f => f.ShouldNotBeNullOrEmpty("Filename is required and cannot be empty.")) ??
           ValidationStatus.Invalid("Filename is required and cannot be empty.");

    public ValidationStatus IsValid(AliasQueryResult alias)
    {
        var rules = new List<ValidationStatus>
        {
            IsNameValid(alias.Name),
            IsFileNameValid(alias.FileName),
            AreNamesUnique(alias.Synonyms, alias.Id),
            IsDeleted(alias.Synonyms, alias.Id),
            AreNamesNotStoreShortcuts(alias.Synonyms)
        };

        if (rules.All(x => x.IsSuccess)) { return ValidationStatus.Valid(); }

        var errors = rules.Where(x => !x.ErrorContent.IsNullOrWhiteSpace())
                          .Select(x => x.ErrorContent);
        var strErrors = string.Join(Environment.NewLine, errors);
        return ValidationStatus.Invalid(strErrors);
    }

    #endregion
}