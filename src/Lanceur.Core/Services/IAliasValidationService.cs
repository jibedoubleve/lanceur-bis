using Lanceur.Core.Models;
using Lanceur.Core.Services.Validators;

namespace Lanceur.Core.Services;

public interface IAliasValidationService
{
    #region Methods

    ValidationStatus IsFileNameValid(object fileName);
    ValidationStatus AreNamesUnique(object names, long idAlias);
    ValidationStatus IsNameValid(object name);
    ValidationStatus IsValid(AliasQueryResult alias);

    #endregion
}