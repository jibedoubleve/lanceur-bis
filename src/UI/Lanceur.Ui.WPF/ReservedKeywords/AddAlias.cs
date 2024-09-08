using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("add")]
[Description("Add a new alias")]
public class AddAlias : SelfExecutableQueryResult
{
    #region Properties

    public override string Icon => "AddCircle24";

    #endregion Properties

    public AddAlias(IServiceProvider serviceProvider) { }

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        if (cmdline is null) return NoResultAsync;


        return NoResultAsync;
    }

    #endregion Methods
}