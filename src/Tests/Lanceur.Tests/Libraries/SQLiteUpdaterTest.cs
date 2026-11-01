using System.Reflection;
using System.SQLite.Updater;
using Dapper;
using Lanceur.Tests.Tools;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Libraries;

public sealed class SQLiteUpdaterTest : TestBase
{
    #region Fields

    private static readonly Assembly Asm = Assembly.GetExecutingAssembly();

    private const string Pattern
        = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

    #endregion

    #region Constructors

    public SQLiteUpdaterTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Fact]
    public void When_updating_from_version_zero_Then_scripts_are_applied()
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