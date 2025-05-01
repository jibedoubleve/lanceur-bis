using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Constants;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Services;

public class SQLiteDatabaseConfigurationServiceShould : TestBase
{
    #region Constructors

    public SQLiteDatabaseConfigurationServiceShould(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    private void WithConfiguration(Action<IDatabaseConfigurationService> assert, string json = null)
    {
        var logger = Substitute.For<ILogger<SQLiteDatabaseConfigurationService>>();
        var sql = $"insert into settings (s_key, s_value) values ('json', '{json}');";

        using var c = BuildFreshDb();
        c.Execute(sql);
        using var scope = new DbSingleConnectionManager(c);
        var settingRepository = new SQLiteDatabaseConfigurationService(scope, logger);
        using (new AssertionScope()) { assert(settingRepository); }
    }

    private void WithConfiguration(Action<DatabaseConfiguration> update, Action<DatabaseConfiguration> assert)
    {
        var logger = Substitute.For<ILogger<SQLiteDatabaseConfigurationService>>();
        using var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var settingRepository = new SQLiteDatabaseConfigurationService(scope, logger);

        update(settingRepository.Current);

        settingRepository.Save();
        settingRepository.Load();

        using (new AssertionScope()) { assert(settingRepository.Current); }
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
            json
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
    public void HandleAllProperties()
    {
        const string json = """
                            {
                                "Caching": {
                                    "StoreCacheDuration": 11,
                                    "ThumbnailCacheDuration": 12
                                },
                                "FeatureFlags": [
                                    {
                                        "Description": "aaabbcc",
                                        "Enabled": true,
                                        "FeatureName": "xxyyzz",
                                        "Icon": "ggg"
                                    }
                                ],
                                "Github": {
                                    "LastCheckedVersion": "1.2.3",
                                    "SnoozeVersionCheck": true,
                                    "Token": "123456789"
                                },
                                "HotKey": {
                                    "Key": 55,
                                    "ModifierKey": 56
                                },
                                "Reconciliation": {
                                    "InactivityThreshold": 99,
                                    "LowUsageThreshold": 98
                                },
                                "ResourceMonitor": {
                                    "CpuSmoothingIndex": 66,
                                    "RefreshRate": 77
                                },
                                "SearchBox": {
                                    "SearchDelay": 11.99,
                                    "ShowAtStartup": false,
                                    "ShowLastQuery": false,
                                    "ShowResult": true
                                },
                                "Stores": {
                                    "BookmarkSourceBrowser": "Edge",
                                    "EverythingQuerySuffix": "eee",
                                    "StoreShortcuts": [
                                        {
                                            "AliasOverride": "987654",
                                            "StoreType": "LISBS"
                                        },
                                        {
                                            "AliasOverride": "123456",
                                            "StoreType": "LISES"
                                        }
                                    ]
                                },
                                "Window": {
                                    "BackdropStyle": "Micass",
                                    "NotificationDisplayDuration": 11,
                                    "Position": {
                                        "Left": 1.23456,
                                        "Top": 2.45678
                                    }
                                }
                            }
                            """;
        WithConfiguration(
            assert =>
            {
                assert.Current.Caching.StoreCacheDuration.Should().Be(11);
                assert.Current.Caching.ThumbnailCacheDuration.Should().Be(12);
                if (assert.Current.FeatureFlags.Any())
                {
                    var ff = assert.Current.FeatureFlags.ElementAt(0);
                    ff.Description.Should().Be("aaabbcc");
                    ff.Enabled.Should().Be(true);
                    ff.FeatureName.Should().Be("xxyyzz");
                    ff.Icon.Should().Be("ggg");
                }
                else { Assert.Fail("No feature flags"); }

                assert.Current.Github.LastCheckedVersion.Should().Be(new("1.2.3"));
                assert.Current.Github.SnoozeVersionCheck.Should().Be(true);
                assert.Current.Github.Token.Should().Be("123456789");

                assert.Current.HotKey.Key.Should().Be(55);
                assert.Current.HotKey.ModifierKey.Should().Be(56);

                assert.Current.Reconciliation.InactivityThreshold.Should().Be(99);
                assert.Current.Reconciliation.LowUsageThreshold.Should().Be(98);

                assert.Current.ResourceMonitor.CpuSmoothingIndex.Should().Be(66);
                assert.Current.ResourceMonitor.RefreshRate.Should().Be(77);

                assert.Current.SearchBox.SearchDelay.Should().Be(11.99);
                assert.Current.SearchBox.ShowAtStartup.Should().Be(false);
                assert.Current.SearchBox.ShowLastQuery.Should().Be(false);
                assert.Current.SearchBox.ShowResult.Should().Be(true);

                assert.Current.Stores.BookmarkSourceBrowser.Should().Be("Edge");
                assert.Current.Stores.EverythingQuerySuffix.Should().Be("eee");

                if (assert.Current.Stores.StoreShortcuts.Count() == 2)
                {
                    assert.Current.Stores.StoreShortcuts.ElementAt(0).AliasOverride.Should().Be("987654");
                    assert.Current.Stores.StoreShortcuts.ElementAt(0).StoreType.Should().Be("LISBS");

                    assert.Current.Stores.StoreShortcuts.ElementAt(1).AliasOverride.Should().Be("123456");
                    assert.Current.Stores.StoreShortcuts.ElementAt(1).StoreType.Should().Be("LISES");
                }
                else { Assert.Fail("No store StoreShortcuts"); }

                assert.Current.Window.BackdropStyle.Should().Be("Micass");
                assert.Current.Window.NotificationDisplayDuration.Should().Be(11);
                assert.Current.Window.Position.Left.Should().Be(1.23456);
                assert.Current.Window.Position.Top.Should().Be(2.45678);
            },
            json
        );
    }

    [Fact]
    public void HaveDefaultHotKey()
    {
        WithConfiguration(repository =>
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
        WithConfiguration(repository =>
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
        var logger = Substitute.For<ILogger<SQLiteDatabaseConfigurationService>>();
        var settings = new SQLiteDatabaseConfigurationService(scope, logger);

        settings.Current.SearchBox.ShowAtStartup.Should().BeTrue();
    }

    [Fact]
    public void HaveDefaultShowResult()
    {
        var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var logger = Substitute.For<ILogger<SQLiteDatabaseConfigurationService>>();
        var settings = new SQLiteDatabaseConfigurationService(scope, logger);

        settings.Current.SearchBox.ShowResult.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(HaveUpdatedPropertyFeed))]
    public void HaveUpdatedProperty(Action<DatabaseConfiguration> update, Action<DatabaseConfiguration> assert)
        => WithConfiguration(update, assert);

    public static IEnumerable<object[]> HaveUpdatedPropertyFeed()
    {
        yield return
        [
            new Action<DatabaseConfiguration>(cfg  =>  cfg.Caching.StoreCacheDuration = 99),
            new Action<DatabaseConfiguration>(cfg => cfg.Caching.StoreCacheDuration.Should().Be(99))
        ];
        yield return
        [
            new Action<DatabaseConfiguration>(cfg  =>  cfg.Caching.ThumbnailCacheDuration = 99),
            new Action<DatabaseConfiguration>(cfg => cfg.Caching.ThumbnailCacheDuration.Should().Be(99))
        ];
        yield return
        [
            new Action<DatabaseConfiguration>(cfg  =>  cfg.Github.Tag = "hello world"),
            new Action<DatabaseConfiguration>(cfg => cfg.Github.Tag.Should().Be("hello world"))
        ];
        yield return
        [
            new Action<DatabaseConfiguration>(cfg  =>  cfg.Github.Tag = cfg.Github.Tag),
            new Action<DatabaseConfiguration>(cfg => cfg.Github.Tag.Should().Be("ungroomed"))
        ];
    }


    [Theory]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    public void SaveHotKey(int modifierKey, int key)
    {
        WithConfiguration(repository =>
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
        WithConfiguration(repository =>
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