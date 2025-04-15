using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Tests.Tooling.ReservedAliases;

namespace Lanceur.Tests.Tools.ReservedAliases;

[ReservedAlias(Names.Name1)]
[Description("description")]
public class ExecutableTestAlias : MacroQueryResult
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public ExecutableTestAlias(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Name = Guid.NewGuid().ToString()[..8];
    }

    #endregion

    #region Methods

    public override SelfExecutableQueryResult Clone() => new ExecutableTestAlias(_serviceProvider);

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        Parameters = cmdline?.Parameters;
        return NoResultAsync;
    }

    #endregion
}