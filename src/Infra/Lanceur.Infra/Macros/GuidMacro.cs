using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Macros;

[Macro("guid"), Description("Creates a guid and save it into the clipboard")]
public class GuidMacro : MacroQueryResult
{
    #region Fields

    private readonly IClipboardService _memoryMemento;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public GuidMacro(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _memoryMemento = serviceProvider.GetService<IClipboardService>();
    }

    #endregion

    #region Properties

    public override string Icon => "Code24";

    #endregion

    #region Methods

    public override SelfExecutableQueryResult Clone() => new GuidMacro(_serviceProvider);

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        _memoryMemento.SaveText(Guid.NewGuid().ToString());
        return NoResultAsync;
    }

    #endregion
}