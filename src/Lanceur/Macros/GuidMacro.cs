using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Lanceur.Core;
using Lanceur.Core.Models;

namespace Lanceur.Macros
{
    [Macro("guid"), Description("Creates a guid and save it into the clipboard")]
    public class GuidMacro : SelfExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            Clipboard.SetText(Guid.NewGuid().ToString());
            return NoResultAsync;
        }

        #endregion Methods
    }
}