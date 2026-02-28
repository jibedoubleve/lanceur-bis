using Lanceur.Core.Constants;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public class FeatureFlagShould : TestBase
{
    #region Constructors

    public FeatureFlagShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Fact]
    public void TakeDefaultWhenDbIsEmpty()
    {
        var conn = BuildFreshDb();
        var scope = new DbSingleConnectionManager(conn);

        var settings = new SQLiteFeatureFlagService(scope);

        settings.IsEnabled(Features.ResourceDisplay).ShouldBeTrue();
    }

    #endregion
}