using System.Reflection;
using System.SQLite.Updater;
using Dapper;
using Lanceur.Tests.Tools;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Libraries;

public class SQLiteUpdaterShould : TestBase
{
    #region Fields

    private static readonly Assembly Asm = Assembly.GetExecutingAssembly();

    private const string Pattern
        = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

    #endregion

    #region Constructors

    public SQLiteUpdaterShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Fact]
    public void UpdateDatabase()
    {
        var ver = new Version(0, 0);

        using var db = BuildConnection();
        var updater = new DatabaseUpdater(db, Asm, Pattern);

        updater.UpdateFrom(ver);

        var sql = "select count(*) from dummy_table;";
        db.ExecuteScalar<int>(sql).ShouldBe(2);
    }

    #endregion
}