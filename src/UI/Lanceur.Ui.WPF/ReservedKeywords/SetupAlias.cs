using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.ReservedKeywords;
using Lanceur.Ui.WPF.Views.Pages;

namespace Lanceur.ReservedKeywords;

[ReservedAlias("setup")]
[Description("Open the setup page")]
public class SetupAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly PageNavigator _navigator;

    #endregion

    #region Constructors

    public SetupAlias(IServiceProvider serviceProvider)
    {
        _navigator = new(serviceProvider);
    }

    #endregion

    #region Properties

    public override string Icon => "Settings24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateToSettings<KeywordsView>();
        return NoResultAsync;
    }

    #endregion
}