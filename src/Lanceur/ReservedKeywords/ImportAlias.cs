using Lanceur.Core;
using Lanceur.Core.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("import"), Description("Import aliases from Slickrun")]
    public class ImportAlias : SelfExecutableQueryResult
    {
        #region Properties

        public override string Icon => "DatabaseImportOutline";

        #endregion Properties

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null) => NoResultAsync;

        #endregion Methods
    }
}