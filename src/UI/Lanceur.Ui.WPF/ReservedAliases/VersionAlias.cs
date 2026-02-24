using System.ComponentModel;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.WPF.Views.Controls;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("version")]
[Description("Indicates the version of the application")]
public class VersionAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IUserDialogueService _userDialogue;
    private readonly IUserNotificationService _userNotification;

    #endregion

    #region Constructors

    public VersionAlias(IUserDialogueService userDialogue, IUserNotificationService userNotification)
    {
        _userDialogue = userDialogue;
        _userNotification = userNotification;
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

        await _userDialogue.ShowAsync("Lanceur - Version", new VersionView(currentVersion));

        return await NoResultAsync;
    }

    #endregion
}