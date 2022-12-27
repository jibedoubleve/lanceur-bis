using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui;
using Microsoft.Toolkit.Uwp.Notifications;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.Macros
{
    [Macro("debug"), Description("Provides some debugging tools. But it is more an easter egg than something else")]
    public class DebugMacro : ExecutableQueryResult
    {
        #region Constructors

        internal DebugMacro(string name, string description, Cmdline query)
        {
            Name = name;
            Query = query;
            SetDescription(description);
        }

        public DebugMacro()
        { }

        #endregion Constructors

        #region Properties

        private static ICmdlineManager CmdlineProcessor => Locator.Current.GetService<ICmdlineManager>();
        private static IConvertionService Converter => Locator.Current.GetService<IConvertionService>();
        private static IMacroManager MacroManager => Locator.Current.GetService<IMacroManager>();
        private static ISearchService SearchService => Locator.Current.GetService<ISearchService>();

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

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var cl = CmdlineProcessor.BuildFromText(cmdline.Parameters);

            var result = cl.Name.ToLower() switch
            {
                "cache" => DumpCache(),
                "echo" => Echo(cl),
                "all" => SearchService.GetAll(),
                "macro" => Converter.ToQueryResult(MacroManager.GetAll()),
                _ => new List<QueryResult> {
                    new DebugMacro("all" ,  "List all the aliases",  Cmdline("debug all") ),
                    new DebugMacro("echo", "Echo some text in a message box. (This is useless!)",  Cmdline("debug echo") ),
                    new DebugMacro("macro", "Provide the list of all macros",  Cmdline("debug macro") ),
                    new DebugMacro("cache", "Displays thumbnails in the cache",  Cmdline("debug cache") ),
                },
            };
            return Task.FromResult(result);
        }

        private static IEnumerable<QueryResult> DumpCache()
        {
            var cache = Locator.Current.GetService<IImageCache>();
            var results = new List<DisplayQueryResult>();
            foreach (var item in cache)
            {
                var key = item.Key;
                var value = item.Value?.GetType()?.ToString() ?? "NULL";
                results.Add(new DisplayQueryResult(value, key, "image"));
            }
            return results;
        }

        public override string ToQuery() => $"debug {Query?.Parameters}".Trim().ToLower();

        #endregion Methods
    }
}