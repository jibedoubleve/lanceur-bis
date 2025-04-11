using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.ReservedKeywords;

[ReservedAlias("setup")]
[Description("Open the setup page")]
public class SetupAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly INavigationService _navigator;

    #endregion

    #region Constructors

    public SetupAlias(IServiceProvider serviceProvider) => _navigator = serviceProvider.GetService<INavigationService>()!;

    #endregion

    #region Properties

    public override string Icon => "Settings24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _navigator.NavigateToKeywords();
        return NoResultAsync;
    }

    #endregion
}