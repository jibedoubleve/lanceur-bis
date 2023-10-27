using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using System.Reflection;

namespace Lanceur.Infra.Managers
{
    public class MacroManager : MacroManagerCache, IMacroManager
    {
        #region Constructors

        public MacroManager(Assembly asm, IAppLoggerFactory logFactory = null) : base(asm, logFactory)
        {
        }

        #endregion Constructors

        #region Methods

        /// <inheritdoc/>
        public IEnumerable<string> GetAll() => MacroInstances.Keys;

        /// <inheritdoc/>
        public IEnumerable<QueryResult> Handle(IEnumerable<QueryResult> collection)
        {
            return collection
                .Select(item => Handle(item))
                .Where(item => item is not null);
        }

        /// <inheritdoc/>
        public QueryResult Handle(QueryResult item)
        {
            if (item is not AliasQueryResult alias || !alias.IsMacro())
            {
                return item;
            }

            if (!MacroInstances.ContainsKey(alias.GetMacroName()))
            {
                /* Well, this is a misconfigured macro, log it and forget it */
                Log.Warning($"User has misconfigured a Macro with name '{alias.FileName}'. Fix the name of the macro or remove the alias from the database.");
                return null;
            }

            var macro = MacroInstances[alias.GetMacroName()];
            macro.Name = alias.Name;
            macro.Parameters = alias.Parameters;
            return macro;
        }

        #endregion Methods
    }
}