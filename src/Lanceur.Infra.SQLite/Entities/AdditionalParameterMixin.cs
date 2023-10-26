using Lanceur.Core.Models;

namespace Lanceur.Infra.SQLite.Entities;

public static class AdditionalParameterMixin
{
    #region Methods

    public static IEnumerable<AdditionalParameter>
        ToEntity(this IEnumerable<QueryResultAdditionalParameters> collection, long idAlias) => collection
        .Select(item => new AdditionalParameter { Id = item.Id, IdAlias = idAlias, Name = item.Name, Parameter = item.Parameter })
        .ToList();

    #endregion Methods
}