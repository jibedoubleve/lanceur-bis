using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Macros;

[Macro("clipboard_as_text")]
[Description("Remove formatting from clipboard.")]
public class ClipboardAsTextMacro : MacroQueryResult
{
    #region Fields

    private readonly IClipboardService _clipboard;

    #endregion

    #region Constructors

    public ClipboardAsTextMacro(IClipboardService clipboard)
    {
        _clipboard = clipboard;
    }

    #endregion

    #region Properties

    public override string Icon => "ClipboardCode24";

    #endregion

    #region Methods

    public override SelfExecutableQueryResult Clone() => new ClipboardAsTextMacro(_clipboard);

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var text = _clipboard.RetrieveText();
        _clipboard.SaveText(text);
        return NoResultAsync;
    }

    #endregion
}