using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.ReservedKeywords;

[ReservedAlias("setup")]
[Description("Open the setup page")]
public class SetupAlias : SelfExecutableQueryResult
{
    private readonly IServiceProvider _serviceProvider;

    #region Constructors

    public SetupAlias(IServiceProvider serviceProvider) { _serviceProvider = serviceProvider; }

    #endregion

    #region Properties

    public override string Icon => "Settings24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var view = _serviceProvider.GetService<SettingsView>()!;
        view.Show();
        return NoResultAsync;
    }

    #endregion
}