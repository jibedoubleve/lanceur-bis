using Lanceur.Constants;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("setup"), Description("Open the setup page")]
    public class SetupAlias : SelfExecutableQueryResult
    {
        #region Properties

        public override string Icon => "CogOutline";

        #endregion Properties

        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var view = new SettingsView();
            view.ViewModel.PushNavigation.Execute(SettingsRoutes.Keywords).Subscribe();
            view.Show();
            return NoResultAsync;
        }

        #endregion Methods
    }
}