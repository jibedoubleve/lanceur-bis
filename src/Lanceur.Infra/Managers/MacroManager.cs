using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using System.Reflection;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Utils;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Managers
{
    public class MacroManager : MacroManagerCache, IMacroManager
    {
        #region Constructors

        public MacroManager(Assembly asm, IAppLoggerFactory logFactory = null, IDbRepository repository = null) : base(asm, logFactory, repository)
        {
        }

        #endregion Constructors

        #region Methods

        /// <inheritdoc/>
        public IEnumerable<string> GetAll() => MacroInstances.Keys;

        /// <inheritdoc/>
        public IEnumerable<QueryResult> Handle(QueryResult[] collection)
        {
            var result =  collection.Select(Handle)
                                    .Where(item => item is not null)
                                    .ToArray();
            return result;
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

            var macro = MacroInstances[alias.GetMacroName()].Clone();
            macro.Name = alias.Name;
            macro.Parameters = alias.Parameters;
            return macro;
        }

        #endregion Methods
    }
}