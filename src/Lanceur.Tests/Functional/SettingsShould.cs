using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Infra;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Tests.SQLite;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class SettingsShould : SQLiteTest
    {
        #region Methods

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
        public void SaveJsonData()
        {
            var file = Path.GetTempFileName();
            var stg = new JsonSettingsService(file);

            stg[Setting.DbPath] = "undeuxtrois";
            stg.Save();

            var json = File.ReadAllText(file);

            json.Should().Be("{\"DbPath\":\"undeuxtrois\"}");
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
        public void Load()
        {
            var conn = BuildFreshDB();
            var scope = new SQLiteConnectionScope(conn);
            var settings = new SQLiteAppSettingsService(scope);

            settings.Load();
        }

        #endregion Methods
    }
}