using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Generators;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Version = System.Version;

namespace Lanceur.Tests.Services;

public sealed class SQLiteDatabaseConfigurationServiceTest : TestBase
{
    #region Constructors

    public SQLiteDatabaseConfigurationServiceTest(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    private void WithConfiguration(
        Action<ISettingsProvider<ApplicationSettings>> assert, 
        string? json = null, 
        IConnectionString? connectionString = null)
    {
        var sql = $"insert into settings (s_key, s_value) values ('json', '{json}');";

        connectionString ??= ConnectionStringFactory.InMemory;

        using var c = BuildFreshDb(connectionString: connectionString);
        c.Execute(sql);
        using var scope = new DbSingleConnectionManager(c);
        var settingRepository = new SQLiteApplicationSettingsProvider(
            scope,
            CreateLogger<SQLiteApplicationSettingsProvider>()
        );
        assert(settingRepository);
    }

    private void WithConfiguration(Action<ApplicationSettings> update, Action<ApplicationSettings> assert)
    {
        using var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var settingRepository = new SQLiteApplicationSettingsProvider(
            scope,
            CreateLogger<SQLiteApplicationSettingsProvider>()
        );

        update(settingRepository.Value);

        settingRepository.Save();
        settingRepository.Load();

        assert(settingRepository.Value);
    }

    public static IEnumerable<object[]> HaveUpdatedPropertyFeed()
    {
        yield return
        [
            new Action<ApplicationSettings>(cfg => cfg.Caching.StoreCacheDuration = 99),
            new Action<ApplicationSettings>(cfg => cfg.Caching.StoreCacheDuration.ShouldBe(99))
        ];
        yield return
        [
            new Action<ApplicationSettings>(cfg => cfg.Caching.ThumbnailCacheDuration = 99),
            new Action<ApplicationSettings>(cfg => cfg.Caching.ThumbnailCacheDuration.ShouldBe(99))
        ];
        yield return
        [
            new Action<ApplicationSettings>(cfg => cfg.Github.Tag = "hello world"),
            new Action<ApplicationSettings>(cfg => cfg.Github.Tag.ShouldBe("hello world"))
        ];
        yield return
        [
            new Action<ApplicationSettings>(_ => { }),
            new Action<ApplicationSettings>(cfg => cfg.Github.Tag.ShouldBe("ungroomed"))
        ];
    }

    [Fact]
    public void When_deserialising_feature_flags_Then_obsoletes_are_deleted()
    {
        var features = Features.GetNames();
        var json = $$"""
                     {
                         "FeatureFlags": [
                             {
                                 "Description": "{{Generate.Text()}}",
                                 "Enabled": false,
                                 "FeatureName": "{{Generate.Text()}}",
                                 "Icon": "{{Generate.Text()}}"
                             },
                             {
                                 "Description": "{{Generate.Text()}}",
                                 "Enabled": true,
                                 "FeatureName": "{{Generate.Text()}}",
                                 "Icon": "{{Generate.Text()}}"
                             },
                             {
                                 "Description": "{{Generate.Text()}}",
                                 "Enabled": true,
                                 "FeatureName": "{{Generate.Text()}}",
                                 "Icon": "{{Generate.Text()}}"
                             }
                         ]
                     }
                     """;
        WithConfiguration(repository =>
                repository.Value.FeatureFlags.ToList()
                          .ShouldSatisfyAllConditions(
                              ff => ff.Count.ShouldBeGreaterThan(0),
                              ff => ff.Select(f => features.Contains(f.FeatureName))
                                      .Count().ShouldBe(features.Count())
                          ),
            json);
    }

    [Fact]
    public void When_no_db_Then_a_fresh_one_is_created()
    {
        var file = Path.GetTempFileName();
        var stg = new JsonInfrastructureSettingsProvider(file);
        File.Delete(file);

        var value = stg.Value.Database.DbPath;

        value.ShouldBe(Paths.DefaultDb);
    }

    [Fact]
    public void When_retrieving_default_settings_Then_default_position_has_the_expected_value() =>
        WithConfiguration(repository => {
                var settings = repository.Value;
                settings.Window.Position.Left.ShouldBe(double.MaxValue);
                settings.Window.Position.Top.ShouldBe(double.MaxValue);
            }
        );

    [Fact]
    public void When_retrieving_default_settings_Then_HotKey_has_the_expected_value() =>
        WithConfiguration(repository => {
                var settings = repository.Value;
                settings.HotKey.ModifierKey.ShouldBe(3);
                settings.HotKey.Key.ShouldBe(18);
            }
        );

    [Fact]
    public void When_retrieving_default_settings_Then_ShowAtStartup_is_set_to_true()
    {
        var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var settings = new SQLiteApplicationSettingsProvider(scope, CreateLogger<SQLiteApplicationSettingsProvider>());

        settings.Value.SearchBox.ShowAtStartup.ShouldBeTrue();
    }

    [Fact]
    public void When_retrieving_default_settings_Then_ShowResult_is_set_to_true()
    {
        var c = BuildFreshDb();
        using var scope = new DbSingleConnectionManager(c);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();
        var settings = new SQLiteApplicationSettingsProvider(scope, logger);

        settings.Value.SearchBox.ShowResult.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void When_updating_feature_flas_Then_json_is_updated(bool isEnabled)
    {
        var description = Generate.Text();
        var featureName = Features.GetNames().ElementAt(0);
        var icon = Generate.Text();

        var json = $$"""
                     {
                         "FeatureFlags": [
                             {
                                 "Description": "{{description}}",
                                 "Enabled": {{(isEnabled ? "true" : "false")}},
                                 "FeatureName": "{{featureName}}",
                                 "Icon": "{{icon}}"
                             },
                             {
                                 "Description": "Enables administrator",
                                 "Enabled": true,
                                 "FeatureName": "AdminMode1",
                                 "Icon": "ShieldKeyhole241"
                             }
                         ]
                     }
                     """;
        WithConfiguration(repository =>
                repository.Value.FeatureFlags
                          .Single(f => f.FeatureName == featureName)
                          .ShouldSatisfyAllConditions(
                              ff => ff.Enabled.ShouldBe(isEnabled),
                              ff => ff.FeatureName.ShouldBe(featureName),
                              ff => ff.Icon.ShouldBe(icon),
                              ff => ff.Description.ShouldBe(description)
                          ),
            json);
    }


    [Theory]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    public void When_updating_HotKey_Then_it_is_saved_in_db(int modifierKey, int key) =>
        WithConfiguration(repository => {
                var settings = repository.Value;
                settings.HotKey = new HotKeySection(modifierKey, key);

                repository.Save();
                settings.ShouldSatisfyAllConditions(
                    s => s.HotKey.ModifierKey.ShouldBe(modifierKey),
                    s => s.HotKey.Key.ShouldBe(key)
                );
            }
        );

    [Fact]
    public void When_updating_json_Then_it_is_saved_in_db()
    {
        var file = Path.GetTempFileName();
        var stg = new JsonInfrastructureSettingsProvider(file)
        {
            Value =
            {
                Database =
                {
                    DbPath = "un_deux_trois"
                }
            }
        };

        stg.Save();

        var asThisJson = JsonConvert.SerializeObject(stg.Value);
        var json = File.ReadAllText(file);

        json.ShouldBe(asThisJson);
    }

    [Theory]
    [InlineData(1.1d, 2.1d)]
    public void When_updating_position_Then_it_is_saved_in_db(double left, double top) =>
        WithConfiguration(repository => {
                var settings = repository.Value;
                settings.Window.Position.Left = left;
                settings.Window.Position.Top = top;

                repository.Save();

                var loaded = repository.Value;
                loaded.Window.Position.Left.ShouldBe(left);
                loaded.Window.Position.Top.ShouldBe(top);
            }
        );

    [Fact]
    public void When_updating_settings_Then_values_are_updated_when_retrieving()
    {
        var file = Path.GetTempFileName();
        var stg = new JsonInfrastructureSettingsProvider(file);
        var expected = "undeuxtrois";

        stg.Value.Database.DbPath = expected;
        stg.Save();

        var actual = stg.Value.Database.DbPath;

        actual.ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(HaveUpdatedPropertyFeed))]
    public void When_updating_value_Then_it_is_saved_in_db(
        Action<ApplicationSettings> update, Action<ApplicationSettings> assert)
        => WithConfiguration(update, assert);

    [Fact]
    public void When_updating_values_in_settings_Then_retrieving_updated_values()
    {
        var features = Features.GetNames().ToList();
        var description = Generate.Text();
        var icon = Generate.Text();

        var json = $$"""
                     {
                         "Caching": {
                             "StoreCacheDuration": 11,
                             "ThumbnailCacheDuration": 12
                         },
                         "FeatureFlags": [
                             {
                                 "Description": "{{description}}",
                                 "Enabled": true,
                                 "FeatureName": "{{features[0]}}",
                                 "Icon": "{{icon}}"
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
        WithConfiguration(assert =>
                assert.Value.ShouldSatisfyAllConditions(
                    a => a.Caching.StoreCacheDuration.ShouldBe(11),
                    a => a.Caching.ThumbnailCacheDuration.ShouldBe(12),
                    a => {
                        if (a.FeatureFlags.Any())
                        {
                            var ff = assert.Value.FeatureFlags.ElementAt(0);
                            ff.Description.ShouldBe(description);
                            ff.Enabled.ShouldBe(true);
                            ff.FeatureName.ShouldBe(features[0]);
                            ff.Icon.ShouldBe(icon);
                        }
                        else { Assert.Fail("No feature flags"); }
                    },
                    a => a.Github.LastCheckedVersion.ShouldBe(new Version("1.2.3")),
                    a => a.Github.SnoozeVersionCheck.ShouldBe(true),
                    a => a.Github.Token.ShouldBe("123456789"),
                    a => a.HotKey.Key.ShouldBe(55),
                    a => a.HotKey.ModifierKey.ShouldBe(56),
                    a => a.Reconciliation.InactivityThreshold.ShouldBe(99),
                    a => a.Reconciliation.LowUsageThreshold.ShouldBe(98),
                    a => a.ResourceMonitor.CpuSmoothingIndex.ShouldBe(66),
                    a => a.ResourceMonitor.RefreshRate.ShouldBe(77),
                    a => a.SearchBox.SearchDelay.ShouldBe(11.99),
                    a => a.SearchBox.ShowAtStartup.ShouldBe(false),
                    a => a.SearchBox.ShowLastQuery.ShouldBe(false),
                    a => a.SearchBox.ShowResult.ShouldBe(true),
                    a => a.Stores.BookmarkSourceBrowser.ShouldBe("Edge"),
                    a => a.Stores.EverythingQuerySuffix.ShouldBe("eee"),
                    a => {
                        if (a.Stores.StoreShortcuts.Count() == 2)
                        {
                            a.Stores.StoreShortcuts.ElementAt(0).AliasOverride.ShouldBe("987654");
                            a.Stores.StoreShortcuts.ElementAt(0).StoreType.ShouldBe("LISBS");

                            a.Stores.StoreShortcuts.ElementAt(1).AliasOverride.ShouldBe("123456");
                            a.Stores.StoreShortcuts.ElementAt(1).StoreType.ShouldBe("LISES");
                        }
                        else { Assert.Fail("No store StoreShortcuts"); }
                    },
                    a => a.Window.BackdropStyle.ShouldBe("Micass"),
                    a => a.Window.NotificationDisplayDuration.ShouldBe(11),
                    a => a.Window.Position.Left.ShouldBe(1.23456),
                    a => a.Window.Position.Top.ShouldBe(2.45678)
                ),
            json);
    }

    #endregion
}