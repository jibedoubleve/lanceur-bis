using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.Entities;

namespace Lanceur.Infra.Sqlite.Entities;

public static class AdditionalParameterMixin
{
    #region Methods

    public static IEnumerable<AdditionalParameter>
        ToEntity(this IEnumerable<QueryResultAdditionalParameters> collection, long idAlias) => collection
        .Select(item => new AdditionalParameter { Id = item.Id, IdAlias = idAlias, Name = item.Name, Parameter = item.Parameter })
        .ToList();

    #endregion Methods
}