using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;

namespace Lanceur.Infra.Stores;

[Store]
public class SteamGameStore : Store, IStoreService
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;

    private readonly ISteamLibraryService _steamLibraryService;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public SteamGameStore(
        IStoreOrchestrationFactory orchestrationFactory,
        ISteamLibraryService steamLibraryService,
        IAliasManagementService aliasManagementService
    ) : base(orchestrationFactory)
    {
        _steamLibraryService = steamLibraryService;
        _aliasManagementService = aliasManagementService;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => true;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Exclusive(@"^\s{0,}&.*");

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        var games = _steamLibraryService.GetGames();
        var results = games.Where(x => x.Name.Contains(cmdline.Parameters,
                               StringComparison.InvariantCultureIgnoreCase))
                           .Select(x => new AliasQueryResult
                           {
                               Name = x.Name,
                               Description = "Steam library game",
                               FileName = x.ToSteamUrl()
                           }).ToList();
        _aliasManagementService.HydrateSteamGameUsage(results);
        var res = results.OrderByDescending(x => x.Count)
                         .ThenBy(x => x.Name);
        return res;
    }

    #endregion
}