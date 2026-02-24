using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Tests.Tooling.ReservedAliases;

namespace Lanceur.Tests.Tools.ReservedAliases;

[ReservedAlias(Names.Name2)]
[Description("To be a keyword, you should be executable")]
[DebuggerDisplay("Name: {Name}")]
public class NotExecutableTestAlias : QueryResult
{
    #region Constructors

    public NotExecutableTestAlias(IServiceProvider _) => Name = Guid.NewGuid().ToString()[..8];

    #endregion
}