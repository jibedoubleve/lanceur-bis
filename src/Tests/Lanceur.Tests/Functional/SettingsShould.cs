using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Constants;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.SQLite;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Functional
{
    public class SettingsShould : TestBase
    {
        #region Constructors

        public SettingsShould(ITestOutputHelper output) : base(output)
        {
        }

        #endregion Constructors

        #region Methods

        private void WithConfiguration(Action<IAppConfigRepository> assert)
        {
            using var c = BuildFreshDb();
            using var scope = new DbSingleConnectionManager(c);
            var settingRepository = new SQLiteAppConfigRepository(scope);

            assert(settingRepository);
        }

        [Fact]
        public void CreateFileWhenNotExists()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonLocalConfigRepository(file);
            File.Delete(file);

            var value = stg.Current.DbPath;

            value.Should().Be(Paths.DefaultDb);
        }

        [Fact]
        public void GetAndSetData()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonLocalConfigRepository(file);
            var expected = "undeuxtrois";

            stg.Current.DbPath = expected;
            stg.Save();

            var actual = stg.Current.DbPath;

            actual.Should().Be(expected);
        }

        [Fact]
        public void HaveDefaultHotKey()
        {
            WithConfiguration(repository =>
            {
                var settings = repository.Current;
                settings.HotKey.ModifierKey.Should().Be(3);
                settings.HotKey.Key.Should().Be(18);
            });
        }

        [Fact]
        public void HaveDefaultPosition()
        {
            WithConfiguration(repository =>
            {
                var settings = repository.Current;
                settings.Window.Position.Left.Should().Be(double.MaxValue);
                settings.Window.Position.Top.Should().Be(double.MaxValue);
            });
        }

        [Fact]
        public void HaveDefaultShowAtStartup()
        {
            var c = BuildFreshDb();
            var scope = new DbSingleConnectionManager(c);
            var settings = new SQLiteAppConfigRepository(scope);

            settings.Current.Window.ShowAtStartup.Should().BeTrue();
        }

        [Fact]
        public void HaveDefaultShowResult()
        {
            var c = BuildFreshDb();
            var scope = new DbSingleConnectionManager(c);
            var settings = new SQLiteAppConfigRepository(scope);

            settings.Current.Window.ShowResult.Should().BeFalse();
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(10, 20)]
        public void SaveHotKey(int modifierKey, int key)
        {
            WithConfiguration(repository =>
            {
                var settings = repository.Current;
                settings.HotKey = new HotKeySection(modifierKey, key);

                repository.Save();
                settings.HotKey.ModifierKey.Should().Be(modifierKey);
                settings.HotKey.Key.Should().Be(key);
            });
        }

        [Fact]
        public void SaveJsonData()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonLocalConfigRepository(file)
            {
                Current =
                {
                    DbPath = "un_deux_trois"
                }
            };

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
            });
        }

        #endregion Methods
    }
}