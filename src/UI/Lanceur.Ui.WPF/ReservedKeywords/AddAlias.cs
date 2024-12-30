using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.Views.Pages;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("add")]
[Description("Add a new alias")]
public class AddAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly PageNavigator _navigator;

    #endregion

    #region Constructors

    public AddAlias(IServiceProvider serviceProvider) => _navigator = new(serviceProvider);

    #endregion

    #region Properties

    public override string Icon => "AddCircle24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.Navigate<KeywordsView>();
        WeakReferenceMessenger.Default.Send(new AddAliasMessage(cmdline));
        return NoResultAsync;
    }

    #endregion
}

