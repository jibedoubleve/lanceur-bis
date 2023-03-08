using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Tests.SQLite;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class SettingsShould : SQLiteTest
    {
        #region Methods

        private static void Assert(Action<IAppSettingsService> assert)
        {
            var connection = BuildFreshDB();
            var scope = new SQLiteConnectionScope(connection);
            var settingRepository = new SQLiteAppSettingsService(scope);

            assert(settingRepository);
        }

        [Fact]
        public void CreateFileWhenNotExists()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonSettingsService(file);
            File.Delete(file);

            var value = stg[Setting.DbPath];

            value.Should().Be(@"%appdata%\probel\lanceur2\data.sqlite");
        }

        [Fact]
        public void GetAndSetData()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonSettingsService(file);
            var expected = "undeuxtrois";

            stg[Setting.DbPath] = expected;
            stg.Save();

            var actual = stg[Setting.DbPath];

            actual.Should().Be(expected);
        }

        [Fact]
        public void HaveDefaultHotKey()
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.HotKey.ModifierKey.Should().Be(3);
                settings.HotKey.Key.Should().Be(18);
            });
        }

        [Fact]
        public void HaveDefaultPosition()
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.Window.Position.Left.Should().Be(600);
                settings.Window.Position.Top.Should().Be(150);
            });
        }

        [Fact]
        public void Load()
        {
            var conn = BuildFreshDB();
            var scope = new SQLiteConnectionScope(conn);
            var settings = new SQLiteAppSettingsService(scope);

            settings.Load();
        }

        [Theory]
        [InlineData(1, 2)]
        public void SaveHotKey(int modifierKey, int key)
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.HotKey = new HotKeySection(modifierKey, key);

                repository.Save(settings);
                settings.HotKey.ModifierKey.Should().Be(modifierKey);
                settings.HotKey.Key.Should().Be(key);
            });
        }

        [Fact]
        public void SaveJsonData()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonSettingsService(file);

            stg[Setting.DbPath] = "undeuxtrois";
            stg.Save();

            var json = File.ReadAllText(file);

            json.Should().Be("{\"DbPath\":\"undeuxtrois\"}");
        }

        [Theory]
        [InlineData(1.1d, 2.1d)]
        public void SavePosition(double left, double top)
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.Window.Position.Left = left;
                settings.Window.Position.Top = top;

                repository.Save(settings);

                var loaded = repository.Load();
                loaded.Window.Position.Left.Should().Be(left);
                loaded.Window.Position.Top.Should().Be(top);
            });
        }

        #endregion Methods
    }
}