using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using Splat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Lanceur.Macros.Development
{
    [Macro("fixthumb"), Description("Fix thumbnails for the Packaged Apps")]
    public class FixThumbnails : MacroQueryResult
    {
        #region Fields

        private readonly IDbRepository _aliasService = Locator.Current.GetService<IDbRepository>();
        private readonly INotification _notify = Locator.Current.GetService<INotification>();
        private readonly IPackagedAppValidator _validator = Locator.Current.GetService<IPackagedAppValidator>();

        #endregion Fields

        #region Methods

        public override SelfExecutableQueryResult Clone() => this.CloneObject();

        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var aliases = _aliasService.GetAll().ToArray();
            var counter = 0;

            for (var i = 0; i < aliases.Length; i++)
            {
                if (!aliases[i].IsPackagedApplication()) continue;

                await _validator.FixAsync(aliases[i]);
                _aliasService.SaveOrUpdate(ref aliases[i]);
                counter++;
            }

            _notify.Information($"Updated thumbnails for {counter} packaged application(s).");
            return NoResult;
        }

        #endregion Methods
    }
}