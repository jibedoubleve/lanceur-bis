using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.ReservedKeywords;

[ReservedAlias("add")]
[Description("Add a new alias")]
public class AddAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IMessageService _messenger;

    private readonly INavigationService _navigationService;

    #endregion

    #region Constructors

    public AddAlias(IServiceProvider serviceProvider)
    {
        _navigationService = serviceProvider.GetService<INavigationService>()!;
        _messenger = serviceProvider.GetService<IMessageService>()!;
    }

    #endregion

    #region Properties

    public override string Icon => "AddCircle24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigationService.NavigateToKeywords();
        _messenger.Send(new AddAliasMessage(cmdline));
        return NoResultAsync;
    }

    #endregion
}