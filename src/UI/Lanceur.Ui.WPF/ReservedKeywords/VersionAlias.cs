using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui.WPF.Views.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("version")]
[Description("Indicates the version of the application")]
public class VersionAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IUserInteractionService _userInteraction;
    private readonly IUserNotificationService _userNotification;

    #endregion

    #region Constructors

    public VersionAlias(IServiceProvider serviceProvider)
    {
        _userInteraction = serviceProvider.GetService<IUserInteractionService>()!;
        _userNotification = serviceProvider.GetService<IUserNotificationService>()!;
    }

    #endregion

    #region Properties

    public override string Icon => "BookInformation24";

    #endregion

    #region Methods

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _userNotification.DisableLoadingState();
        var asm = Assembly.GetExecutingAssembly();
        var fullVer = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        var semverSplit = fullVer?.Split(["+"], StringSplitOptions.RemoveEmptyEntries);
        
        var ver = semverSplit?.Length > 0 ? semverSplit[0] : fullVer;
        var commit = semverSplit?.Length > 0 ? semverSplit[1] : string.Empty;

        await _userInteraction.ShowAsync("Lanceur - Version", new VersionView(ver!, commit!));

        return await NoResultAsync;
    }

    #endregion
}