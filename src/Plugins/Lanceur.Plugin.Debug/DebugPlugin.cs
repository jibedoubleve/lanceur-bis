using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using System.ComponentModel;

namespace Lanceur.Plugin.Debug
{
    [Plugin("pdeb"), Description("Plugin just used for plugin debugging.")]
    public class DebugPlugin : PluginBase
    {
        #region Methods

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(string? parameters = null)
        {
            Log.Trace($"Executing plugin '{nameof(DebugPlugin)}'");

            var i = 0;
            IEnumerable<QueryResult> result = new List<QueryResult>()
            {
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
                new DisplayQueryResult($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "Carabiner"),
            };
            return Task.FromResult(result);
        }

        #endregion Methods
    }
}