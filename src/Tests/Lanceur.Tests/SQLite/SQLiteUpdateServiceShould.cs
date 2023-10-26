using Dapper;
using FluentAssertions;
using Lanceur.Infra.SQLite;
using System.Data.SQLite;
using Xunit;

namespace Lanceur.Tests.SQLite
{
    public class SQLiteUpdateServiceShould : SQLiteTest
    {
        #region Methods

        private static void CreateTable(SQLiteConnection db)
        {
            var ddl = @"
            create table settings (
                id      integer primary key,
                s_key   text,
                s_value text
            );";
            db.Execute(ddl);
        }

        [Theory]
        [InlineData("1.9")]
        [InlineData("1.8")]
        [InlineData("1.7")]
        public void BeUpToDate(string goal)
        {
            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);
            CreateVersion(db, "1.0");

            var service = new SQLiteVersionManager(db);
            service.IsUpToDate(goal)
                   .Should().BeFalse();
        }

        [Theory]
        [InlineData("error")]
        [InlineData("2")]
        [InlineData("")]
        [InlineData(null)]
        public void CrashWhenInvalidVersionAsStringIsSpecified(string version)
        {
            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);
            CreateVersion(db, "1.0");

            var service = new SQLiteVersionManager(db);
            var action = () => service.IsUpToDate(version);
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("1.0", "1.0")]
        [InlineData("1.1", "1.1")]
        [InlineData("1.1.1", "1.1.1")]
        [InlineData("1.1.1.1", "1.1.1.1")]
        public void HaveUpToDateDatabaseWhenVersionAsStringIsSpecified(string expected, string actual)
        {
            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);
            CreateVersion(db, actual);

            var service = new SQLiteVersionManager(db);
            service.IsUpToDate(expected).Should().BeTrue();
        }

        [Theory]
        [InlineData("1.0", "1.0")]
        [InlineData("1.1", "1.1")]
        [InlineData("1.1.1", "1.1.1")]
        [InlineData("1.1.1.1", "1.1.1.1")]
        public void HaveUpToDateDatabaseWhenVersionIsSpecified(string goal, string actual)
        {
            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);
            CreateVersion(db, actual);

            var goalVersion = new Version(goal);
            var service = new SQLiteVersionManager(db);
            service.IsUpToDate(goalVersion).Should().BeTrue();
        }

        [Theory]
        [InlineData("1.0")]
        [InlineData("1.1")]
        [InlineData("2.0")]
        [InlineData("2.1")]
        [InlineData("2.1.1")]
        public void ReturnVersionOfDatabase(string version)
        {
            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);
            CreateVersion(db, version);

            var service = new SQLiteVersionManager(db);
            var expected = new Version(version);

            service.GetCurrentDbVersion().Should().Be(expected);
        }

        [Theory]
        [InlineData("1.2")]
        [InlineData("1.2.3")]
        [InlineData("1.2.3.4")]
        public void SetDatabaseVersion(string ver)
        {
            var version = new Version(ver);

            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);

            var service = new SQLiteVersionManager(db);
            service.SetCurrentDbVersion(version);
            service.GetCurrentDbVersion().Should().Be(version);
        }

        [Theory]
        [InlineData("0.9.9")]
        [InlineData("0.9")]
        [InlineData("0.8")]
        [InlineData("0.7")]
        public void Update(string goal)
        {
            using var db = new SQLiteDbConnectionManager(BuildConnection());
            CreateTable(db);
            CreateVersion(db, "1.0");

            var service = new SQLiteVersionManager(db);
            service.IsUpToDate(goal)
                   .Should().BeTrue();
        }

        #endregion Methods
    }
}