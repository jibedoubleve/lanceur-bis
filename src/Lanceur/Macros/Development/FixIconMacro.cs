using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Splat;

namespace Lanceur.Macros.Development;

[Macro("fixicon"), Description("Fix icons when alias is a directory or an hyperlink")]
public class FixIconMacro : SelfExecutableQueryResult
{
    #region Properties

    public override string Icon => "AutoFix";

    #endregion Properties

    #region Methods

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var action = Locator.Current.GetService<IDataDoctorRepository>();
        await action.FixIconsForHyperlinksAsync();

        return NoResult;
    }

    #endregion Methods
}