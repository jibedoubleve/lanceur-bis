using System.Diagnostics;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Services;

public class ReconciliationService : IReconciliationService
{
    #region Fields

    private readonly IPackagedAppSearchService _packagedAppSearchService;

    #endregion

    #region Constructors

    public ReconciliationService(IPackagedAppSearchService packagedAppSearchService) => _packagedAppSearchService = packagedAppSearchService;

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task ProposeDescriptionAsync(AliasQueryResult alias)
    {
        if (alias.FileName.IsNullOrEmpty()) return;
        if (!File.Exists(alias.FileName)) return;
        if (await _packagedAppSearchService.TryResolveDetailsAsync(alias)) return;

        alias.Description = FileVersionInfo.GetVersionInfo(alias.FileName)
                                           .FileDescription;
    }

    /// <inheritdoc />
    public async Task ProposeDescriptionAsync(IEnumerable<AliasQueryResult> aliases)
    {
        foreach (var alias in aliases) await ProposeDescriptionAsync(alias);
    }

    #endregion
}