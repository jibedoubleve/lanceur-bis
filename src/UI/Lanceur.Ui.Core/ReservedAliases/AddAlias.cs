using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.Core.Messages;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("add")]
[Description("Add a new alias")]
public sealed class AddAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly ISettingsNavigationService _navigator;

    #endregion

    #region Constructors

    public AddAlias(ISettingsNavigationService navigator) => _navigator = navigator;

    #endregion

    #region Properties

    public override string Icon => "AddCircle24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateTo<KeywordsViewModel>();
        WeakReferenceMessenger.Default.Send(new AddAliasMessage(cmdline));
        return NoResultAsync;
    }

    #endregion
}