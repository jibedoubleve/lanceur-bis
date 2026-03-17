using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;

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

    /// <inheritdoc />
    public void Delete(AliasQueryResult alias) => _repository.RemoveLogically(alias);

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> GetAll()
    {
        var results = _repository.GetAll()
                                 .Where(x => !x.IsHidden)
                                 .OrderBy(x => x.Name)
                                 .ToArray();
        foreach (var result in results)
        {
            result.MarkUnchanged();
        }

        return results;
    }

    /// <inheritdoc />
    public AliasQueryResult Hydrate(AliasQueryResult queryResult)
    {
        _repository.HydrateAlias(queryResult);
        queryResult.MarkUnchanged();
        return queryResult;
    }

    /// <inheritdoc />
    public IEnumerable<AliasQueryResult> HydrateSteamGameUsage(IEnumerable<AliasQueryResult> aliases) 
        => _repository.HydrateSteamGameUsage(aliases);

    /// <inheritdoc />
    public void SaveOrUpdate(ref AliasQueryResult alias)
    {
        _repository.SaveOrUpdate(ref alias);
        alias.MarkUnchanged();
    }

    /// <inheritdoc />
    public void UpdateThumbnail(AliasQueryResult alias) => _repository.UpdateThumbnail(alias);

    #endregion
}