using Dapper;
using FluentAssertions;
using System.Reflection;
using System.SQLite.Updater;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.SQLite
{
    public class DatabaseUpdaterShould : TestBase
    {
        #region Fields

        private const string pattern = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";
        private static readonly Assembly asm = Assembly.GetExecutingAssembly();

        #endregion Fields

        #region Constructors

        public DatabaseUpdaterShould(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        #endregion Constructors

        #region Methods

        [Fact]
        public void UpdateDatabase()
        {
            var ver = new Version(0, 0);

            using var db = BuildConnection();
            var updater = new DatabaseUpdater(db, asm, pattern);

            updater.UpdateFrom(ver);

            var sql = "select count(*) from dummy_table;";
            db.ExecuteScalar<int>(sql).Should().Be(2);
        }

        #endregion Methods
    }
}