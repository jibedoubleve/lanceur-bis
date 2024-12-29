using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.Entities;
using AdditionalParameter = Lanceur.Core.Models.AdditionalParameter;

namespace Lanceur.Infra.Sqlite.Entities;

public static class AdditionalParameterMixin
{
    #region Methods

    public static IEnumerable<SQLite.Entities.AdditionalParameter>
        ToEntity(this IEnumerable<AdditionalParameter> collection, long idAlias) => collection.Select(
                                                                                                              item => new SQLite.Entities.AdditionalParameter { Id        = item.Id, IdAlias   = idAlias, Name      = item.Name, Parameter = item.Parameter }
                                                                                                          )
                                                                                                          .ToList();

    #endregion Methods
}