using Lanceur.Core;
using Lanceur.Core.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("import"), Description("Import aliases from Slickrun")]
    public class ImportAlias : ExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null) => NoResultAsync;

        #endregion Methods
    }
}