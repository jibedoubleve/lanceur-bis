using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public class AliasManagementService : IAliasManagementService
{
    #region Fields

    private readonly IAliasRepository _repository;

    #endregion

    #region Constructors

    public AliasManagementService(IAliasRepository repository) => _repository = repository;

    #endregion

    #region Methods

    public void Delete(AliasQueryResult alias) => _repository.RemoveLogically(alias);

    public IEnumerable<AliasQueryResult> GetAll()
    {
        var results = _repository.GetAll()
                                 .Where(x => x.IsHidden == false)
                                 .OrderBy(x => x.Name)
                                 .ToArray();
        foreach (var result in results) result.MarkUnchanged();
        return results;
    }

    public AliasQueryResult GetById(long id) => _repository.GetById(id);

    public AliasQueryResult Hydrate(AliasQueryResult queryResult)
    {
        _repository.HydrateAlias(queryResult);
        queryResult.MarkUnchanged();
        return queryResult;
    }

    public void SaveOrUpdate(ref AliasQueryResult alias)
    {
        _repository.SaveOrUpdate(ref alias);
        alias.MarkUnchanged();
    }

    #endregion
}