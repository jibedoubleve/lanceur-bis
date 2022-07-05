using Lanceur.Core;
using Lanceur.Core.Models;
using System.ComponentModel;
using System.Diagnostics;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    [ReservedAlias("Invalid"), Description("To be a keyword, you should be executable")]
    [DebuggerDisplay("Name: {Name}")]
    public class NotExecutableTestAlias : QueryResult
    {
        public static NotExecutableTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };
    }
}