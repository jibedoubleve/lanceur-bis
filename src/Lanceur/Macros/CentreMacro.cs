using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Utils;
using Splat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords
{
    [Macro("centre"), Description("Center Lanceur in the middle of the screen")]
    public class CentreMacro : SelfExecutableQueryResult
    {
        #region Methods

        private static void Save(Coordinate coordinate)
        {
            var stg = Locator.Current.GetService<IAppSettingsService>();
            stg.Edit(s =>
            {
                s.Window.Position.Left = coordinate.X;
                s.Window.Position.Top = coordinate.Y;
            });
        }

        public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            var parameters = cmdline?.Parameters.IsNullOrEmpty() ?? true
                ? ScreenRuler.DefaultTopOffset.ToString()
                : cmdline.Parameters;

            _ = int.TryParse(parameters, out int offset);

            var coordinate = ScreenRuler.GetCenterCoordinate(offset);
            AppLogFactory.Get<CentreMacro>().Trace($"Put the screen at the centre of the screen. (x: {coordinate.X} - y: {coordinate.Y} - offset: {offset})");

            ScreenRuler.SetWindowPosition(coordinate);

            Save(coordinate);
            return NoResultAsync;
        }

        #endregion Methods
    }
}