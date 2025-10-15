using System.Data;
using Dapper;
using Shouldly;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tools;
using Xunit;

namespace Lanceur.Tests.Repositories;

public class SQLiteVersionServiceShould : TestBase
{
    #region Constructors

    public SQLiteVersionServiceShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static void CreateTable(IDbConnection db)
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
        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());
        CreateVersion(db.GetConnection(), "1.0");

        var service = new SQLiteVersionService(db);
        service.IsUpToDate(goal)
               .ShouldBeFalse();
    }

    [Theory]
    [InlineData("error")]
    [InlineData("2")]
    [InlineData("")]
    [InlineData(null)]
    public void CrashWhenInvalidVersionAsStringIsSpecified(string version)
    {
        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());
        CreateVersion(db.GetConnection(), "1.0");

        var service = new SQLiteVersionService(db);
        Should.Throw<ArgumentException>(() => service.IsUpToDate(version));
    }

    [Theory]
    [InlineData("1.0", "1.0")]
    [InlineData("1.1", "1.1")]
    [InlineData("1.1.1", "1.1.1")]
    [InlineData("1.1.1.1", "1.1.1.1")]
    public void HaveUpToDateDatabaseWhenVersionAsStringIsSpecified(string expected, string actual)
    {
        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());
        CreateVersion(db.GetConnection(), actual);

        var service = new SQLiteVersionService(db);
        service.IsUpToDate(expected).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1.0", "1.0")]
    [InlineData("1.1", "1.1")]
    [InlineData("1.1.1", "1.1.1")]
    [InlineData("1.1.1.1", "1.1.1.1")]
    public void HaveUpToDateDatabaseWhenVersionIsSpecified(string goal, string actual)
    {
        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());
        CreateVersion(db.GetConnection(), actual);

        var goalVersion = new Version(goal);
        var service = new SQLiteVersionService(db);
        service.IsUpToDate(goalVersion).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("1.1")]
    [InlineData("2.0")]
    [InlineData("2.1")]
    [InlineData("2.1.1")]
    public void ReturnVersionOfDatabase(string version)
    {
        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());
        CreateVersion(db.GetConnection(), version);

        var service = new SQLiteVersionService(db);
        var expected = new Version(version);

        service.GetCurrentDbVersion().ShouldBe(expected);
    }

    [Theory]
    [InlineData("1.2")]
    [InlineData("1.2.3")]
    [InlineData("1.2.3.4")]
    public void SetDatabaseVersion(string ver)
    {
        var version = new Version(ver);

        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());

        var service = new SQLiteVersionService(db);
        service.SetCurrentDbVersion(version);
        service.GetCurrentDbVersion().ShouldBe(version);
    }

    [Theory]
    [InlineData("0.9.9")]
    [InlineData("0.9")]
    [InlineData("0.8")]
    [InlineData("0.7")]
    public void Update(string goal)
    {
        using var db = new DbSingleConnectionManager(BuildConnection());
        CreateTable(db.GetConnection());
        CreateVersion(db.GetConnection(), "1.0");

        var service = new SQLiteVersionService(db);
        service.IsUpToDate(goal)
               .ShouldBeTrue();
    }

    #endregion
}