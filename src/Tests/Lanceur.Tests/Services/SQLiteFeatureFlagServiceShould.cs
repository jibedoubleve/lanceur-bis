using Dapper;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public class SQLiteFeatureFlagServiceShould : TestBase
{
    #region Constructors

    public SQLiteFeatureFlagServiceShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CheckFeatureFlag(bool value)
    {
        // arrange
        var conn = BuildFreshDb();
        var scope = new DbSingleConnectionManager(conn);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();

        var settings = new SQLiteApplicationSettingsProvider(scope, logger);
        var featureFlag = new SQLiteFeatureFlagService(scope);

        settings.Current.FeatureFlags.ShouldNotBeEmpty("application has feature flags");
        settings.Current.FeatureFlags.ElementAt(0).Enabled.ShouldBeTrue("this is the default value");

        // act
        settings.Current.FeatureFlags.ElementAt(0).Enabled = value;
        settings.Save();

        // assert
        featureFlag.IsEnabled(Features.ResourceDisplay).ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateFeatureFlag(bool value)
    {
        // arrange
        var conn = BuildFreshDb();

        var scope = new DbSingleConnectionManager(conn);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();
        var settings = new SQLiteApplicationSettingsProvider(scope, logger);

        settings.Current.FeatureFlags.ShouldNotBeEmpty("application has feature flags");
        settings.Current.FeatureFlags.ElementAt(0).Enabled.ShouldBeTrue("this is the default value");

        // act
        settings.Current.FeatureFlags.ElementAt(0).Enabled = value;
        settings.Save();

        // assert
        settings.Current.FeatureFlags
                .Single(e => e.FeatureName.Equals(Features.ResourceDisplay, StringComparison.InvariantCultureIgnoreCase)
                )
                .Enabled.ShouldBe(value, "the new value changed");

        const string sql = """
                           select s_value ->> '$.FeatureFlags' as FeatureFlag
                           from settings
                           where s_key = 'json'
                           """;
        var json = conn.Query<string>(sql).Single();
        var flags = JsonConvert.DeserializeObject<IEnumerable<FeatureFlag>>(json);

        var flag = flags.SingleOrDefault(e => e.FeatureName.Equals(
                Features.ResourceDisplay,
                StringComparison.InvariantCultureIgnoreCase
            )
        );
        flag.ShouldNotBeNull();
        flag!.FeatureName.ShouldBe(Features.ResourceDisplay);
        flag.Enabled.ShouldBe(value);
    }

    #endregion
}