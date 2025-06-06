using FluentAssertions;
using Lanceur.Core.Constants;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Xunit;
using Xunit.Abstractions;

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

        settings.IsEnabled(Features.ResourceDisplay).Should().BeTrue();
    }

    #endregion
}