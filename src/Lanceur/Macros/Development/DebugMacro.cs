using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui;
using Lanceur.Utils;
using Microsoft.Toolkit.Uwp.Notifications;
using Splat;

namespace Lanceur.Macros.Development
{
    [Macro("debug"), Description("Provides some debugging tools. But it is more an easter egg than something else")]
    public class DebugMacro : SelfExecutableQueryResult
    {
        #region Constructors

        internal DebugMacro(string name, string description, Cmdline query)
        {
            Name = name;
            Query = query;
            Description = description;
        }

        public DebugMacro()
        { }

        #endregion Constructors

        #region Properties

        private static ICmdlineManager CmdlineProcessor => Locator.Current.GetService<ICmdlineManager>();
        private static IConvertionService Converter => Locator.Current.GetService<IConvertionService>();
        private static IMacroManager MacroManager => Locator.Current.GetService<IMacroManager>();
        private static ISearchService SearchService => Locator.Current.GetService<ISearchService>();

        public override string Icon => "BugOutline";

        #endregion Properties

        #region Methods

        private static Cmdline Cmdline(string cmd) => CmdlineProcessor.BuildFromText(cmd);

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
            var cl = CmdlineProcessor.BuildFromText(cmdline?.Parameters ?? string.Empty);

            var result = cl.Name.ToLower() switch
            {
                "cache" => DumpCache(),
                "echo" => Echo(cl),
                "all" => SearchService.GetAll(),
                "macro" => Converter.ToQueryResult(MacroManager.GetAll()),
                _ => new List<QueryResult> {
                    new DebugMacro("debug all" ,  "List all the aliases",  Cmdline("debug all") ),
                    new DebugMacro("debug echo", "Echo some text in a message box. (This is useless!)",  Cmdline("debug echo") ),
                    new DebugMacro("debug macro", "Provide the list of all macros",  Cmdline("debug macro") ),
                    new DebugMacro("debug cache", "Displays thumbnails in the cache",  Cmdline("debug cache") ),
                },
            };
            AppLogFactory.Get<DebugMacro>().Debug($"Executed 'debug {cl.Name.ToLower()}' and found {result.Count()} item(s)");
            return Task.FromResult(result);
        }

        public override string ToQuery() => $"debug {Query?.Parameters}".Trim().ToLower();

        #endregion Methods
    }
}