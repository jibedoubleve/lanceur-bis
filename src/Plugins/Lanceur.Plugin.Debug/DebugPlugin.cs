﻿using Lanceur.Core.Plugins;
using Lanceur.Core.Plugins.Models;
using System.ComponentModel;

namespace Lanceur.Plugin.Debug;

[Plugin("debug"), Description("Plugin just used for plugin debugging.")]
public class DebugPlugin : PluginBase
{
    #region Methods

    public override Task<IEnumerable<ResultItem>> ExecuteAsync(string? parameters = null)
    {
        var i = 0;
        IEnumerable<ResultItem> result = new List<ResultItem>()
        {
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight"),
            new($"Debug result {++i}", $"Some description and a guid '{Guid.NewGuid()}'", "ChevronRight")
        };
        return Task.FromResult(result);
    }

    #endregion Methods
}