using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Tests.SQLite;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
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
        public void HaveDefaultPosition()
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.Window.Position.Left.Should().Be(600);
                settings.Window.Position.Top.Should().Be(150);
            });
        }

        [Theory]
        [InlineData(1, 2)]
        public void SavePosition(int left, int top)
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.Window.Position.Left = left;
                settings.Window.Position.Top = top;

                repository.Save(settings);
                settings.Window.Position.Left.Should().Be(left);
                settings.Window.Position.Top.Should().Be(top);
            });
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
        public void HaveDefaultHotKey()
        {
            Assert(repository =>
            {
                var settings = repository.Load();
                settings.HotKey.ModifierKey.Should().Be(3);
                settings.HotKey.Key.Should().Be(18);
            });
        }

        #endregion Methods
    }
}