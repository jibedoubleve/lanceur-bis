using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.WPF.Views;
using Lanceur.Ui.WPF.Views.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("add")]
[Description("Add a new alias")]
public class AddAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public AddAlias(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    #endregion

    #region Properties

    public override string Icon => "AddCircle24";

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