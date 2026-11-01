using Dapper;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class SQLiteFeatureFlagServiceTest : TestBase
{
    #region Constructors

    public SQLiteFeatureFlagServiceTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_flag_is_saved_Then_service_returns_new_value(bool value)
    {
        // arrange
        var conn = BuildFreshDb();
        var scope = new DbSingleConnectionManager(conn);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();

        var settings = new SQLiteApplicationSettingsProvider(scope, logger);
        var ffRepository = new SQLiteFeatureFlagRepository(scope);
        var featureFlag = new FeatureFlagService(ffRepository);

        settings.Value.FeatureFlags.ShouldNotBeEmpty("application has feature flags");
        settings.Value.FeatureFlags.ElementAt(0).Enabled.ShouldBeTrue("this is the default value");

        // act
        settings.Value.FeatureFlags.ElementAt(0).Enabled = value;
        settings.Save();

        // assert
        featureFlag.IsEnabled(Features.ResourceDisplay).ShouldBe(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_flag_is_saved_Then_new_value_is_persisted_in_db(bool value)
    {
        // arrange
        var conn = BuildFreshDb();

        var scope = new DbSingleConnectionManager(conn);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();
        var settings = new SQLiteApplicationSettingsProvider(scope, logger);

        settings.Value.FeatureFlags.ShouldNotBeEmpty("application has feature flags");
        settings.Value.FeatureFlags.ElementAt(0).Enabled.ShouldBeTrue("this is the default value");

        // act
        settings.Value.FeatureFlags.ElementAt(0).Enabled = value;
        settings.Save();

        // assert
        settings.Value.FeatureFlags
                .Single(
                    e => e.FeatureName.Equals(Features.ResourceDisplay, StringComparison.InvariantCultureIgnoreCase)
                ).Enabled.ShouldBe(value, "the new value changed");

        const string sql = """
                           select s_value ->> '$.FeatureFlags' as FeatureFlag
                           from settings
                           where s_key = 'json'
                           """;
        var json = conn.Query<string>(sql).Single();
        var flags = JsonConvert.DeserializeObject<IEnumerable<FeatureFlag>>(json)!;

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