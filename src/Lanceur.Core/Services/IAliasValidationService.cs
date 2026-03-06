using Lanceur.Core.Models;
using Lanceur.Core.Services.Validators;

namespace Lanceur.Core.Services;

public interface IAliasValidationService
{
    #region Methods

    /// <summary>
    ///     Determines whether the specified file name is a non-empty string.
    /// </summary>
    /// <param name="fileName">The file name to validate. Must be a non-null, non-empty string.</param>
    /// <returns>
    ///     A <see cref="ValidationStatus" /> indicating success if the file name is valid,
    ///     or failure with an error message if it is null, empty, or not a string.
    /// </returns>
    ValidationStatus IsFileNameValid(object? fileName);

    /// <summary>
    ///     Validates all properties of the specified alias, including its name, file name,
    ///     name uniqueness, and whether any of its names belong to a deleted alias.
    /// </summary>
    /// <param name="alias">The alias to validate.</param>
    /// <returns>
    ///     A <see cref="ValidationStatus" /> indicating success if all rules pass,
    ///     or failure with a combined error message listing all validation issues.
    /// </returns>
    ValidationStatus IsValid(AliasQueryResult alias);

    #endregion
}