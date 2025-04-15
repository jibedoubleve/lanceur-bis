using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Constants;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Services;

public class SQLiteDatabaseConfigurationServiceShould : TestBase
{
    #region Constructors

    public SQLiteDatabaseConfigurationServiceShould(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    private void WithConfiguration(Action<IDatabaseConfigurationService> assert, string sql = null)
    {
        using var c = BuildFreshDb();
        if (sql is not null) c.Execute(sql);
        using var scope = new DbSingleConnectionManager(c);
        var settingRepository = new SQLiteDatabaseConfigurationService(scope);
        using (new AssertionScope()) { assert(settingRepository); }
    }

    [Fact]
    public void CreateFileWhenNotExists()
    {
        var file = Path.GetTempFileName();
        var stg = new JsonApplicationConfigurationService(file);
        File.Delete(file);

        var value = stg.Current.DbPath;

        value.Should().Be(Paths.DefaultDb);
    }

    [Fact]
    public void DeserialiseFeatureFlags()
    {
        const string json = """
                            { "FeatureFlags": [ { "Description": "Show CPU", "Enabled": false, "FeatureName": "ShowSystemUsage1", "Icon": "Gauge241" }, { "Description": "Enables administrator", "Enabled": true, "FeatureName": "AdminMode1", "Icon": "ShieldKeyhole241" } ] }
                            """;
        const string sql = $"insert into settings (s_key, s_value) values ('json', '{json}');";
        WithConfiguration(
            repository =>
            {
                var settings = repository.Current;
                settings.FeatureFlags.Should().HaveCount(2);
                
                settings.FeatureFlags.ElementAt(0).Enabled.Should().BeFalse();
                settings.FeatureFlags.ElementAt(0).FeatureName.Should().Be("ShowSystemUsage1");
                settings.FeatureFlags.ElementAt(0).Icon.Should().Be("Gauge241");
                settings.FeatureFlags.ElementAt(0).Description.Should().Be("Show CPU");
                
                settings.FeatureFlags.ElementAt(1).Enabled.Should().BeTrue();
                settings.FeatureFlags.ElementAt(1).FeatureName.Should().Be("AdminMode1");
                settings.FeatureFlags.ElementAt(1).Icon.Should().Be("ShieldKeyhole241");
                settings.FeatureFlags.ElementAt(1).Description.Should().Be("Enables administrator");
            },
            sql
        );
    }

    [Fact]
    public void GetAndSetData()
    {
        var file = Path.GetTempFileName();
        var stg = new JsonApplicationConfigurationService(file);
        var expected = "undeuxtrois";

        stg.Current.DbPath = expected;
        stg.Save();

        var actual = stg.Current.DbPath;

        actual.Should().Be(expected);
    }

    [Fact]
    public void HaveDefaultHotKey()
    {
        WithConfiguration(
            repository =>
            {
                var settings = repository.Current;
                settings.HotKey.ModifierKey.Should().Be(3);
                settings.HotKey.Key.Should().Be(18);
            }
        );
    }

    [Fact]
    public void HaveDefaultPosition()
    {
        WithConfiguration(
            repository =>
            {
                var settings = repository.Current;
                settings.Window.Position.Left.Should().Be(double.MaxValue);
                settings.Window.Position.Top.Should().Be(double.MaxValue);
            }
        );
    }

    [Fact]
    public void HaveDefaultShowAtStartup()
    {
        var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var settings = new SQLiteDatabaseConfigurationService(scope);

        settings.Current.SearchBox.ShowAtStartup.Should().BeTrue();
    }

    [Fact]
    public void HaveDefaultShowResult()
    {
        var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var settings = new SQLiteDatabaseConfigurationService(scope);

        settings.Current.SearchBox.ShowResult.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    public void SaveHotKey(int modifierKey, int key)
    {
        WithConfiguration(
            repository =>
            {
                var settings = repository.Current;
                settings.SetHotKey(key, modifierKey);

                repository.Save();
                settings.HotKey.ModifierKey.Should().Be(modifierKey);
                settings.HotKey.Key.Should().Be(key);
            }
        );
    }

    [Fact]
    public void SaveJsonData()
    {
        var file = Path.GetTempFileName();
        var stg = new JsonApplicationConfigurationService(file) { Current = { DbPath = "un_deux_trois" } };

        stg.Save();

        var asThisJson = JsonConvert.SerializeObject(stg.Current);
        var json = File.ReadAllText(file);

        json.Should().Be(asThisJson);
    }

    [Theory]
    [InlineData(1.1d, 2.1d)]
    public void SavePosition(double left, double top)
    {
        WithConfiguration(
            repository =>
            {
                var settings = repository.Current;
                settings.Window.Position.Left = left;
                settings.Window.Position.Top = top;

                repository.Save();

                var loaded = repository.Current;
                loaded.Window.Position.Left.Should().Be(left);
                loaded.Window.Position.Top.Should().Be(top);
            }
        );
    }

    #endregion
}