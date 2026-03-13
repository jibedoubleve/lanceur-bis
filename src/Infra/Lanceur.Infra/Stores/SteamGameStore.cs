using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;

namespace Lanceur.Infra.Stores;

[Store(@"^\s{0,}&.*")]
public class SteamGameStore : StoreBase, IStoreService
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ISteamLibraryService _steamLibraryService;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public SteamGameStore(
        IStoreOrchestrationFactory orchestrationFactory,
        ISteamLibraryService steamLibraryService,
        IAliasManagementService aliasManagementService,
        IFeatureFlagService featureFlagService,
        ISection<StoreSection> storeSettings
    ) : base(orchestrationFactory, storeSettings)
    {
        _steamLibraryService = steamLibraryService;
        _aliasManagementService = aliasManagementService;
        _featureFlagService = featureFlagService;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => true;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration
        => _featureFlagService.IsEnabled(Features.SteamIntegration)
            ? StoreOrchestrationFactory.Exclusive(DefaultShortcut)
            : StoreOrchestrationFactory.AlwaysInactive();

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