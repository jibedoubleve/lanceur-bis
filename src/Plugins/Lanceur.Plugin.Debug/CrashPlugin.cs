using Lanceur.Core.Plugins;
using Lanceur.Core.Plugins.Models;
using System.ComponentModel;

namespace Lanceur.Plugin.Debug;

[Plugin("crash"), Description("Just throw an exception to check how the application behaves")]
public class CrashPlugin : PluginBase
{
    #region Methods

    public override Task<IEnumerable<ResultItem>> ExecuteAsync(string? parameters = null) => throw new NotSupportedException("Just throw an error...");

    #endregion Methods
}