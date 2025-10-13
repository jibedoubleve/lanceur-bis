using System.Reflection;
using System.SQLite.Updater;
using Dapper;
using FluentAssertions;
using Lanceur.Tests.Tools;
using Xunit;

namespace Lanceur.Tests.Libraries;

public class SQLiteUpdaterShould : TestBase
{
    #region Fields

    private const string Pattern = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";
    private static readonly Assembly Asm = Assembly.GetExecutingAssembly();

    #endregion Fields

    #region Constructors

    public SQLiteUpdaterShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion Constructors

    #region Methods

    [Fact]
    public void UpdateDatabase()
    {
        var ver = new Version(0, 0);

        using var db = BuildConnection();
        var updater = new DatabaseUpdater(db, Asm, Pattern);

        updater.UpdateFrom(ver);

        var sql = "select count(*) from dummy_table;";
        db.ExecuteScalar<int>(sql).Should().Be(2);
    }

    #endregion Methods
}