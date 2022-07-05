using Lanceur.Core;
using Lanceur.Core.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.Macros
{
    [Macro("stat"), Description("Show the usage statistics of Lanceur")]
    public class StatisticsMacro : ExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            return NoResultAsync;
        }

        #endregion Methods
    }
}