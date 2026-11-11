using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("setup")]
[Description("Open the setup page")]
public sealed class SetupAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ISettingsNavigationService _navigator;

    #endregion

    #region Constructors

    public SetupAlias(ISettingsNavigationService navigator) => _navigator = navigator;

    #endregion

    #region Properties

    public override string Icon => "Settings24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateTo<KeywordsViewModel>();
        return NoResultAsync;
    }

    #endregion
}