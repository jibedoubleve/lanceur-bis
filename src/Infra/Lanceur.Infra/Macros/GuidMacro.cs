using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Macros;

[Macro("guid")]
[Description("Creates a guid and save it into the clipboard")]
public class GuidMacro : MacroQueryResult
{
    #region Fields

    private readonly IClipboardService _memoryMemento;

    #endregion

    #region Constructors
    
    public GuidMacro(IClipboardService memoryMemento) => _memoryMemento = memoryMemento!;

    #endregion

    #region Properties

    public override string Icon => "Code24";

    #endregion

    #region Methods

    public override SelfExecutableQueryResult Clone() => new GuidMacro(_memoryMemento);

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _memoryMemento.SaveText(Guid.NewGuid().ToString());
        return NoResultAsync;
    }

    #endregion
}