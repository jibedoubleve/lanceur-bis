using Lanceur.Core;
using Lanceur.Core.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Lanceur.Tests.Utils.ReservedAliases
{
    [ReservedAlias("Invalid"), Description("To be a keyword, you should be executable")]
    [DebuggerDisplay("Name: {Name}")]
    public class NotExecutableTestAlias : QueryResult
    {
        public NotExecutableTestAlias()
        {
            Name = Guid.NewGuid().ToString().Substring(0,8);
        }
        public static NotExecutableTestAlias FromName(string name) => new() { Name = name, Query = new Cmdline(name) };
    }
}