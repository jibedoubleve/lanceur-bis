﻿using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using Splat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Lanceur.Macros.Development;

[Macro("debug", false), Description("Provides some debugging tools. But it is more an easter egg than something else")]
public class DebugMacro : MacroQueryResult
{
    #region Fields

    private readonly ILogger<DebugMacro> _logger;

    #endregion Fields

    #region Constructors

    internal DebugMacro(string name, string description, Cmdline query)
    {
        Name = name;
        Query = query;
        Description = description;
        _logger = Locator.Current.GetService<LoggerFactory>().GetLogger<DebugMacro>();
    }

    public DebugMacro() { }

    #endregion Constructors

    #region Properties

    private static ICmdlineManager CmdlineProcessor => Locator.Current.GetService<ICmdlineManager>();
    private static IConversionService Converter => Locator.Current.GetService<IConversionService>();
    private static IMacroManager MacroManager => Locator.Current.GetService<IMacroManager>();
    private static IAsyncSearchService SearchService => Locator.Current.GetService<IAsyncSearchService>();

    public override string Icon => "BugOutline";

    #endregion Properties

    #region Methods

    private static Cmdline Cmdline(string cmd) => CmdlineProcessor.BuildFromText(cmd);

    private static IEnumerable<QueryResult> Echo(Cmdline cl)
    {
        new ToastContentBuilder()
            .AddText("Lanceur: easter egg")
            .AddText($"You wanted to say: '{cl.Parameters}'")
            .Show();
        return NoResult;
    }

    public override SelfExecutableQueryResult Clone() => this.CloneObject();

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var cl = CmdlineProcessor.BuildFromText(cmdline?.Parameters ?? string.Empty);

        var result = cl.Name.ToLower() switch
        {
            "echo"  => Echo(cl),
            "all"   => await SearchService.GetAllAsync(),
            "macro" => Converter.ToQueryResult(MacroManager.GetAll()),
            _       => new List<QueryResult>
            {
                new DebugMacro("debug all",  "List all the aliases",  Cmdline("debug all")),
                new DebugMacro("debug echo", "Echo some text in a message box. (This is useless!)",  Cmdline("debug echo")),
                new DebugMacro("debug macro", "Provide the list of all macros",  Cmdline("debug macro"))
            }
        };
        result = result.ToList();
        Locator.Current.GetLogger<DebugMacro>().LogDebug("Executed 'debug {Name}' and found {Result} item(s)", cl.Name.ToLower(), result.Count());
        return result;
    }

    public override string ToQuery() => $"debug {Query?.Parameters}".Trim().ToLower();

    #endregion Methods
}