using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Tests.Tooling.ReservedAliases;

[ReservedAlias("anothertest"), Description("description")]
public class ExecutableTestAlias : MacroQueryResult
{
    private readonly IServiceProvider _serviceProvider;

    #region Constructors

    public ExecutableTestAlias(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Name = Guid.NewGuid().ToString()[..8];
    }

    #endregion Constructors

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        Parameters = cmdline.Parameters;
        return NoResultAsync;
    }

    public override SelfExecutableQueryResult Clone() => new ExecutableTestAlias(_serviceProvider);

    #endregion Methods
}