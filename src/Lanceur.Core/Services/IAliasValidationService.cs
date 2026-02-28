using Lanceur.Core.Models;
using Lanceur.Core.Services.Validators;

namespace Lanceur.Core.Services;

public interface IAliasValidationService
{
    #region Methods

    ValidationStatus AreNamesUnique(object names, long idAlias);

    ValidationStatus IsFileNameValid(object fileName);
    ValidationStatus IsNameValid(object name);
    ValidationStatus IsValid(AliasQueryResult alias);

    #endregion
}