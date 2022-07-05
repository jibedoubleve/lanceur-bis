using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("add"), Description("Add a new alias")]
    public class AddAlias : ExecutableQueryResult
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            if (cmdline is not null)
            {
                var view = new SettingsView();
                view.Show();
                view.ViewModel.AddAlias.Execute(cmdline.Parameters).Subscribe();
            }
            return NoResultAsync;
        }

        #endregion Methods
    }
}