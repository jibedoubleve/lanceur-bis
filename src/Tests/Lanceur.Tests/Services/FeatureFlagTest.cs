using Lanceur.Core.Constants;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class FeatureFlagTest : TestBase
{
    #region Constructors

    public FeatureFlagTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Fact]
    public void When_database_is_empty_Then_default_flag_value_is_used()
    {
        var conn = BuildFreshDb();
        var scope = new DbSingleConnectionManager(conn);
        var ffRepository = new SQLiteFeatureFlagRepository(scope);
        var settings = new FeatureFlagService(ffRepository);

        settings.IsEnabled(Features.ResourceDisplay).ShouldBeTrue();
    }

    #endregion
}