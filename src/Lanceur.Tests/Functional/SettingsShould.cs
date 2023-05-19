using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services.Config;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Tests.SQLite;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class SettingsShould : SQLiteTest
    {
        #region Methods

        private static void WithConfiguration(Action<IAppConfigService> assert)
        {
            using var connection = BuildFreshDB();
            using var scope = new SQLiteConnectionScope(connection);
            var settingRepository = new SQLiteAppConfigService(scope);

            assert(settingRepository);
        }

        [Fact]
        public void CreateFileWhenNotExists()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonDatabaseConfigService(file);
            File.Delete(file);

            var value = stg.Current.DbPath;

            value.Should().Be(@"%appdata%\probel\lanceur2\data.sqlite");
        }

        [Fact]
        public void GetAndSetData()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonDatabaseConfigService(file);
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
                settings.Window.Position.Left.Should().Be(600);
                settings.Window.Position.Top.Should().Be(150);
            });
        }

        [Fact]
        public void HaveDefaultShowAtStartup()
        {
            var conn = BuildFreshDB();
            var scope = new SQLiteConnectionScope(conn);
            var settings = new SQLiteAppConfigService(scope);

            settings.Current.ShowAtStartup.Should().BeTrue();
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
            var stg = new JsonDatabaseConfigService(file);

            stg.Current.DbPath = "undeuxtrois";
            stg.Save();

            var json = File.ReadAllText(file);

            json.Should().Be("{\"DbPath\":\"undeuxtrois\"}");
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