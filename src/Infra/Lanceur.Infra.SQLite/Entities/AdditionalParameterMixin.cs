using Lanceur.Infra.SQLite.Entities;
using AdditionalParameter = Lanceur.Core.Models.AdditionalParameter;

namespace Lanceur.Infra.Sqlite.Entities;

public static class AdditionalParameterMixin
{
    #region Methods

    public static IEnumerable<AdditionalParameterEntity>
        ToEntity(this IEnumerable<AdditionalParameter> collection, long idAlias) => collection.Select(
                                                                                                  item => new AdditionalParameterEntity
                                                                                                  {
                                                                                                      Id = item.Id,
                                                                                                      IdAlias = idAlias,
                                                                                                      Name = item.Name,
                                                                                                      Parameter = item.Parameter
                                                                                                  }
                                                                                              )
                                                                                              .ToList();

    #endregion
}