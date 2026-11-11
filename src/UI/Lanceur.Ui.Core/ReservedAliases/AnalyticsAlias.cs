using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("stat")]
[Description("Shows analytics about the usage of Lanceur")]
public sealed class AnalyticsAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ISettingsNavigationService _navigator;

    #endregion

    #region Constructors

    public AnalyticsAlias(ISettingsNavigationService navigator) => _navigator = navigator;

    #endregion

    #region Properties

    public override string Icon => "ChartMultiple24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateTo<AnalyticsViewModel>();
        return NoResultAsync;
    }

    #endregion
}