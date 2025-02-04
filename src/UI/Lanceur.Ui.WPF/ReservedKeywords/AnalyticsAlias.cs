using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.Views.Pages;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("stat")]
[Description("Shows analytics about the usage of Lanceur")]
public class SettingsViewModel : SelfExecutableQueryResult
{
    #region Fields

    private readonly PageNavigator _navigator;

    #endregion

    #region Constructors

    public SettingsViewModel(IServiceProvider serviceProvider) => _navigator = new(serviceProvider);

    #endregion

    public override string Icon => "ChartMultiple24";

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateToSettings<AnalyticsView>();
        return NoResultAsync;
    }

    #endregion
}