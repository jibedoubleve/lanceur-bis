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

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(string parameter = null)
        {
            if (parameter is not null)
            {
                var view = new SettingsView();
                view.Show();
                view.ViewModel.AddAlias.Execute(parameter).Subscribe();
            }
            return NoResultAsync;
        }

        #endregion Methods
    }
}