using Lanceur.Core;
using Lanceur.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lanceur.ReservedKeywords
{
    [Macro("guid"), Description("Creates a guid and save it into the clipboard")]
    public class GuidMacro : ExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(string parameters = null)
        {
            Clipboard.SetText(Guid.NewGuid().ToString());
            return NoResultAsync;
        }

        #endregion Methods
    }
}