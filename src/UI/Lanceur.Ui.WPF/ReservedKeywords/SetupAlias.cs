using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.WPF.Views;
using Lanceur.Ui.WPF.Views.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.ReservedKeywords;

[ReservedAlias("setup")]
[Description("Open the setup page")]
public class SetupAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public SetupAlias(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    #endregion

    #region Properties

    public override string Icon => "Settings24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        var view = _serviceProvider.GetService<SettingsView>()!;
        view.Show();
        view.Navigate<KeywordsView>();
        return NoResultAsync;
    }

    #endregion
}