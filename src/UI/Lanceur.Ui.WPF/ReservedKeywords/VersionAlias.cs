using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
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
        var semver = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;
        var semverSplit = semver?.Split(["+"], StringSplitOptions.RemoveEmptyEntries);
        semver = semverSplit?.Length > 0 ? semverSplit[0] : semver;

        await _userInteraction.ShowAsync($"Lanceur {semver}", "Written by Jean-Baptiste Wautier");

        return await NoResultAsync;
    }

    #endregion
}