using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace Lanceur.Macros;

[Macro("guid"), Description("Creates a guid and save it into the clipboard")]
public class GuidMacro : MacroQueryResult
{
    #region Properties

    public override string Icon => "SlotMachineOutline";

    #endregion Properties

    #region Methods

    public override SelfExecutableQueryResult Clone() => this.CloneObject();

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        Clipboard.SetText(Guid.NewGuid().ToString());
        return NoResultAsync;
    }

    #endregion Methods
}