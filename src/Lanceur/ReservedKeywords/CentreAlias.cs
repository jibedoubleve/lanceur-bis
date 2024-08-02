using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Win32.Utils;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using Splat;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Lanceur.ReservedKeywords;

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
        stg.Edit(
            s =>
            {
                s.Window.Position.Left = coordinate.X;
                s.Window.Position.Top = coordinate.Y;
            }
        );
    }

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var coordinate = ScreenRuler.SetDefaultPosition();
        StaticLoggerFactory.GetLogger<CentreAlias>()
                           .LogInformation(
                               "Put window at default position. (x: {X} - y: {Y}",
                               coordinate.X,
                               coordinate.Y
                           );

        Save(coordinate);
        return NoResultAsync;
    }

    #endregion Methods
}