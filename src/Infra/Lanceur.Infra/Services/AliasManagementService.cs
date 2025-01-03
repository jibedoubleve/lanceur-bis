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

    public IEnumerable<AliasQueryResult> GetAll() => _repository.GetAll()
                                                                .Where(x => x.IsHidden == false)
                                                                .OrderBy(x => x.Name);

    public AliasQueryResult Hydrate(AliasQueryResult queryResult)
    {
        _repository.HydrateAlias(queryResult);
        return queryResult;
    }

    public void Delete(AliasQueryResult alias) => _repository.Remove(alias);
    public void SaveOrUpdate(ref AliasQueryResult alias) => _repository.SaveOrUpdate(ref alias); 

    #endregion
}