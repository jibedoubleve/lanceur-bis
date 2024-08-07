﻿using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Tests.Tooling.ReservedAliases;

[ReservedAlias("anothertest"), Description("description")]
public class ExecutableTestAlias : MacroQueryResult
{
    #region Constructors

    public ExecutableTestAlias() => Name = Guid.NewGuid().ToString().Substring(0, 8);

    #endregion Constructors

    #region Methods

    public static ExecutableTestAlias FromName(string name) => new() { Name = name, Query = new(name) };

    public static ExecutableTestAlias Random() => FromName(Guid.NewGuid().ToString().Substring(0, 8));

    public override SelfExecutableQueryResult Clone() => this.CloneObject();

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        Parameters = cmdline.Parameters;
        return NoResultAsync;
    }

    #endregion Methods
}