using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("add")]
[Description("Add a new alias")]
public class AddAlias : SelfExecutableQueryResult
{
    private readonly IServiceProvider _serviceProvider;

    #region Properties

    public override string Icon => "AddCircle24";

    #endregion Properties

    public AddAlias(IServiceProvider serviceProvider) { _serviceProvider = serviceProvider; }

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {

        var view = _serviceProvider.GetService<SettingsView>()!;
        view.Show();

        return NoResultAsync;
    }

    #endregion Methods
}