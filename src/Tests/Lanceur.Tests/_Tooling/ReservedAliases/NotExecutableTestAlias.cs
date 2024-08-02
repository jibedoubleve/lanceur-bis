using System.ComponentModel;
using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Tests.Tooling.ReservedAliases;

[ReservedAlias("Invalid"), Description("To be a keyword, you should be executable"), DebuggerDisplay("Name: {Name}")]
public class NotExecutableTestAlias : QueryResult
{
    #region Constructors

    public NotExecutableTestAlias() => Name = Guid.NewGuid().ToString().Substring(0, 8);

    #endregion Constructors

    #region Methods

    public static NotExecutableTestAlias FromName(string name) => new() { Name = name, Query = new(name) };

    #endregion Methods
}