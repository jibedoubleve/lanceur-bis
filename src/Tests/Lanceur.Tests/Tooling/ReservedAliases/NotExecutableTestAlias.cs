using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Tests.Tooling.ReservedAliases;

[ReservedAlias("Invalid")]
[Description("To be a keyword, you should be executable")]
[DebuggerDisplay("Name: {Name}")]
public class NotExecutableTestAlias : QueryResult
{
    #region Constructors

    public NotExecutableTestAlias(IServiceProvider _) => Name = Guid.NewGuid().ToString()[..8];

    #endregion
}