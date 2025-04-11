using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.ReservedKeywords;

[ReservedAlias("stat")]
[Description("Shows analytics about the usage of Lanceur")]
public class SettingsViewModel : SelfExecutableQueryResult
{
    #region Fields

    private readonly INavigationService _navigator;

    #endregion

    #region Constructors

    public SettingsViewModel(IServiceProvider serviceProvider) => _navigator = serviceProvider.GetService<INavigationService>()!;

    #endregion

    #region Properties

    public override string Icon => "ChartMultiple24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateToAnalytics();
        return NoResultAsync;
    }

    #endregion
}