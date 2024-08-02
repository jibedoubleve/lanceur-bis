using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lanceur.ReservedKeywords;

[ReservedAlias("quit"), Description("Quit lanceur")]
public class QuitAlias : SelfExecutableQueryResult
{
    #region Properties

    public override string Icon => "LocationExit";

    #endregion Properties

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        Application.Current.Shutdown();
        this.GetLogger().LogInformation("Quit the application from alias 'Quit'");

        return NoResultAsync;
    }

    #endregion Methods
}