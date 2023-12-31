using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Win32.Utils;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;
using Splat;

namespace Lanceur.ReservedKeywords
{
    [ReservedAlias("centre"), Description("Centre Lanceur in the middle of the screen")]
    public class CentreAlias : SelfExecutableQueryResult
    {
        #region Properties

        public override string Icon => "BookmarkPlusOutline";

        #endregion Properties

        #region Methods

        private static void Save(Coordinate coordinate)
        {
            var stg = Locator.Current.GetService<IAppConfigRepository>();
            stg.Edit(s =>
            {
                s.Window.Position.Left = coordinate.X;
                s.Window.Position.Top = coordinate.Y;
            });
        }

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var parameters = cmdline?.Parameters.IsNullOrEmpty() ?? true
                ? ScreenRuler.DefaultTopOffset.ToString(CultureInfo.InvariantCulture)
                : cmdline.Parameters;

            _ = int.TryParse(parameters, out int offset);

            var coordinate = ScreenRuler.GetCenterCoordinate(offset);
            AppLogFactory.Get<CentreAlias>()
                         .Trace(
                             $"Put the screen at the centre of the screen. (x: {coordinate.X} - y: {coordinate.Y} - offset: {offset})");

            ScreenRuler.SetWindowPosition(coordinate);

            Save(coordinate);
            return NoResultAsync;
        }

        #endregion Methods
    }
}