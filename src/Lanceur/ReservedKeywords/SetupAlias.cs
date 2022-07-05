using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("setup"), Description("Open the setup page")]
    public class SetupAlias : ExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var view = new SettingsView();
            view.Show();
            return NoResultAsync;
        }

        #endregion Methods
    }
}