using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.ReservedKeywords;

[ReservedAlias("setup")]
[Description("Open the setup page")]
public class SetupAlias : SelfExecutableQueryResult
{
    #region Constructors

    public SetupAlias(IServiceProvider serviceProvider) { }

    #endregion

    #region Properties

    public override string Icon => "Settings24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null) => NoResultAsync;

    #endregion
}