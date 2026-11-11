using System.ComponentModel;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Utils;

namespace Lanceur.Ui.Core.ReservedAliases;

[ReservedAlias("version")]
[Description("Indicates the version of the application")]
public sealed class VersionAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IUserDialogueService _userDialogue;
    private readonly IUserNotificationService _userNotification;
    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public VersionAlias(
        IUserDialogueService userDialogue,
        IUserNotificationService userNotification,
        IViewFactory viewFactory
    )
    {
        _userDialogue = userDialogue;
        _userNotification = userNotification;
        _viewFactory = viewFactory;
    }

    #endregion

    #region Properties

    public override string Icon => "BookInformation24";

    #endregion

    #region Methods

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _userNotification.DisableLoadingState();
        var currentVersion = CurrentVersion.FromAssembly(
            Assembly.GetExecutingAssembly()
        );

        await _userDialogue.ShowAsync("Lanceur - Version", _viewFactory.CreateView(currentVersion));

        return await NoResultAsync;
    }

    #endregion
}