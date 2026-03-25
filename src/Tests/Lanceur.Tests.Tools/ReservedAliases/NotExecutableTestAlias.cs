using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Tests.Tools.Helpers;

namespace Lanceur.Tests.Tools.ReservedAliases;

[ReservedAlias(Names.Name2)]
[Description("To be a keyword, you should be executable")]
[DebuggerDisplay("Name: {Name}")]
public sealed class NotExecutableTestAlias : QueryResult
{
    #region Constructors

    public NotExecutableTestAlias(IServiceProvider _) => Name = Any.String(8);

    #endregion
}