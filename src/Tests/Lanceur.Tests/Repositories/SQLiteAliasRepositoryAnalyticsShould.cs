using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Generators;
using Lanceur.Tests.Tools.SQL;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Repositories;

/// <summary>
///     Tests for the analytics and reporting queries of <see cref="SQLiteAliasRepository" />.
///     These tests verify that queries return correct data based on usage history, doubloons, activity, and alias state.
/// </summary>
public class SQLiteAliasRepositoryAnalyticsShould : TestBase
{
    #region Constructors

    public SQLiteAliasRepositoryAnalyticsShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    
    [Fact]
    public void When_SetUsage_called_twice_with_same_fileName_Then_no_doubloon_is_created()
    {
        // arrange
        var connectionManager = new DbSingleConnectionManager(
            BuildFreshDb()
        );
        var loggerFactory = CreateLoggerFactory();
        var dbActionFactory = new DbActionFactory(loggerFactory);

        var alias = new AliasQueryResult()
        {
            Id = 0,
            Name = "test-alias",
            FileName = "some\\path\\tool.exe",
            Count = 0
        } as QueryResult;

        // act — simulate two consecutive executions of the same alias with Id == 0
        connectionManager.WithinTransaction((tx) => {
            var setUsage = new SetUsageDbAction(dbActionFactory);
            setUsage.SetUsage(tx, ref alias);
        });

         alias.Id = 0; // reset as if resolved again without the id

         connectionManager.WithinTransaction(tx => {
             var setUsage = new SetUsageDbAction(dbActionFactory);
             setUsage.SetUsage(tx, ref alias);
         });

        // assert — only one alias row should exist for that file_name
        var count = connectionManager.Connection.ExecuteScalar<int>(
            "select count(*) from alias where file_name = @fileName",
            new { fileName = "some\\path\\tool.exe" }
        );
        count.ShouldBe(1);
    }
    
    private SQLiteAliasRepository BuildRepository(string? sql = null, IConnectionString? connectionString = null)
    {
        var connection = BuildFreshDb(sql, connectionString);
        var scope = new DbSingleConnectionManager(connection);
        var log = CreateLoggerFactory();
        var service = new SQLiteAliasRepository(
            scope,
            log,
            new DbActionFactory(log)
        );
        return service;
    }

    [Fact]
    public void GetAdditionalParameter()
        => Record.Exception(() => BuildRepository().GetAdditionalParameter([1, 2, 3]))
                 .ShouldBeNull();

    [Fact]
    public void GetAliasesWithoutNotes()
    {
        // arrange
        const int count = 2;
        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        var gen = new SqlBuilder();
        var sql = gen.AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetAliasesWithoutNotes()
                             .ToArray();

        aliases.Length.ShouldBe(gen.IdSequence);
        // assert
        aliases.ShouldSatisfyAllConditions(
            col => Assert.All(col, a => a.Count.ShouldBe(2)),
            col => Assert.All(col, a => a.LastUsedAt.ShouldBe(date1))
        );
    }

    [Fact]
    public void GetAll() => Record.Exception(() => BuildRepository().GetAll()).ShouldBeNull();

    [Fact]
    public void GetAllAliasWithAdditionalParameters()
        => Record
           .Exception(()
               => BuildRepository().GetAllAliasWithAdditionalParameters()
           )
           .ShouldBeNull();

    [Theory]
    [InlineData(@"c:\Not\A\Real\File.exe")]
    [InlineData(@"C:\NOT\A\REAL\FILE.EXE")]
    [InlineData(@"c:\not\a\real\file\")]
    [InlineData(@"C:\NOT\A\REAL\FILE\")]
    public void GetBrokenAliases(string fileName)
    {
        // arrange
        const int count = 2;
        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        var gen = new SqlBuilder();
        var sql = gen.AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithFileName($"{fileName}_{gen.IdSequence}")
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithFileName($"{fileName}_{gen.IdSequence}")
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithFileName($"{fileName}_{gen.IdSequence}")
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithFileName($"{fileName}_{gen.IdSequence}")
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithUsage(date1, date2)
                                        .WithFileName($"{fileName}_{gen.IdSequence}")
                                        .WithCount(count)
                                        .WithSynonyms()
                     )
                     .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetBrokenAliases()
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(gen.IdSequence);
        aliases.ShouldSatisfyAllConditions(
            col => Assert.All(col, a => a.Count.ShouldBe(count)),
            col => Assert.All(col, a => a.LastUsedAt.ShouldBe(date1))
        );
    }

    [Fact]
    public void GetById()
        => Record.Exception(() => BuildRepository().GetById(1))
                 .ShouldBeNull();

    [Fact]
    public void GetDeletedAlias()
    {
        // arrange
        const int count = 2;
        var now = DateTime.Now.AddMinutes(-60);
        var date1 = now.AddDays(-1);
        var date2 = now.AddDays(-2);

        var gen = new SqlBuilder();
        var sql = gen.AppendAlias(a => a.WithDeletedAt(now)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithDeletedAt(now)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithDeletedAt(now)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithDeletedAt(now)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms()
                     )
                     .AppendAlias(a => a.WithDeletedAt(now)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms()
                     )
                     .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDeletedAlias()
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(gen.IdSequence);
        aliases.ShouldSatisfyAllConditions(
            col => Assert.All(col, a => a.Count.ShouldBe(2)),
            col => Assert.All(col, a => a.LastUsedAt.ShouldBe(date1))
        );
    }

    [Theory]
    [InlineData("filename", "some arguments")]
    public void GetDoubloons(string filename, string arguments)
    {
        // arrange
        const int count = 2;

        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        const string name = "is_a_doubloon";
        var gen = new SqlBuilder();
        var sql = gen.AppendAlias(a => a.WithFileName(filename)
                                        .WithArguments(arguments)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(filename)
                                        .WithArguments(arguments)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(filename)
                                        .WithArguments(arguments)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(filename)
                                        .WithArguments(arguments)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(filename)
                                        .WithArguments(arguments)
                                        .WithCount(count)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(gen.IdSequence);
        aliases.ShouldSatisfyAllConditions(
            col => Assert.All(col, a => a.Count.ShouldBe(count)),
            col => Assert.All(col, a => a.LastUsedAt.ShouldBe(date1))
        );
    }

    [Fact]
    public void GetDoubloonsWithLuaScript()
    {
        // arrange
        const int count = 2;

        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        const string script1 = "return something";

        const string name = "is_a_doubloon";
        var gen = new SqlBuilder();
        var sql = gen.AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(script1)
                                        .WithSynonyms(name)
                                        .WithUsage(date1, date2)
                     )
                     .AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(script1)
                                        .WithSynonyms(name)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(script1)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(Generate.Text())
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(Generate.Text())
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name)
                     )
                     .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(3);
        aliases.ShouldSatisfyAllConditions(
            col => Assert.All(col, alias => alias.Count.ShouldBe(count)),
            col => Assert.All(col, alias => alias.LastUsedAt.ShouldBe(date1))
        );
    }

    [Fact]
    public void GetDoubloonsWithLuaScriptAndNullScript()
    {
        // arrange
        const int count = 2;

        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        const string script1 = "return something";

        const string name = "is_a_doubloon";
        var sql = new SqlBuilder().AppendAlias(a => a.WithFileName(name)
                                                     .WithArguments(name)
                                                     .WithCount(count)
                                                     .WithLuaScript(null)
                                                     .WithSynonyms(name)
                                                     .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a => a.WithFileName(name)
                                                     .WithArguments(name)
                                                     .WithCount(count)
                                                     .WithLuaScript(script1)
                                                     .WithUsage(date1, date2)
                                                     .WithSynonyms(name)
                                  )
                                  .AppendAlias(a => a.WithFileName(name)
                                                     .WithArguments(name)
                                                     .WithCount(count)
                                                     .WithLuaScript(script1)
                                                     .WithUsage(date1, date2)
                                                     .WithSynonyms(name)
                                  )
                                  .AppendAlias(a => a.WithFileName(name)
                                                     .WithArguments(name)
                                                     .WithCount(count)
                                                     .WithLuaScript(Generate.Text())
                                                     .WithUsage(date1, date2)
                                                     .WithSynonyms(name)
                                  )
                                  .AppendAlias(a => a.WithFileName(name)
                                                     .WithArguments(name)
                                                     .WithCount(count)
                                                     .WithLuaScript(Generate.Text())
                                                     .WithUsage(date1, date2)
                                                     .WithSynonyms(name)
                                  )
                                  .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(2);
        aliases.ShouldSatisfyAllConditions(
            a => Assert.All(a, alias => alias.Count.ShouldBe(count)),
            a => Assert.All(a, alias => alias.LastUsedAt.ShouldBe(date1))
        );
    }

    [Fact]
    public void GetExistingAliases()
    {
        // arrange
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms("name_1"))
                                  .AppendAlias(a => a.WithSynonyms("name_1"))
                                  .AppendAlias(a => a.WithSynonyms("name_1"))
                                  .AppendAlias(a => a.WithSynonyms("name_1"))
                                  .AppendAlias(a => a.WithSynonyms("name_1"))
                                  .ToSql();
        var service = BuildRepository(sql);

        // act
        const string name = "name_1";
        var aliases = service.GetExistingAliases([name, "no_exist", "still_no_exist"], 1)
                             .ToArray();

        // assert
        aliases.ShouldSatisfyAllConditions(
            a => a.Length.ShouldBe(4),
            a => a[0].ShouldBe(name)
        );
    }

    [Fact]
    public void GetExistingDeletedAliases()
        => Record.Exception(() => BuildRepository().GetExistingDeletedAliases([], 0))
                 .ShouldBeNull();

    [Fact]
    public void GetHiddenCounters()
        => Record.Exception(() => BuildRepository().GetHiddenCounters())
                 .ShouldBeNull();

    [Fact]
    public void GetInactiveAliases()
    {
        // arrange
        var date1 = DateTime.Now.AddMonths(-10);
        var date2 = DateTime.Now.AddMonths(-20);
        var sql = new SqlBuilder().AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(DateTime.Now)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetInactiveAliases(1)
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(4);
        aliases.ShouldSatisfyAllConditions(
            a => Assert.All(a, alias => alias.Count.ShouldBe(2)),
            a => Assert.All(a, alias => alias.LastUsedAt.ShouldBe(date1))
        );
    }

    [Fact]
    public void GetRarelyUsedAliases()
    {
        // arrange
        var date1 = DateTime.Now.AddMonths(-10);
        var date2 = DateTime.Now.AddMonths(-20);
        var date3 = DateTime.Now.AddMonths(-30);
        var sql = new SqlBuilder().AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2, date3)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .AppendAlias(a =>
                                      a.WithSynonyms()
                                       .WithUsage(date1, date2)
                                  )
                                  .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetRarelyUsedAliases(3)
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(4);
        foreach (var alias in aliases)
        {
            alias.Count.ShouldBe(2);
            alias.LastUsedAt.ShouldBe(date1);
        }
    }

    [Fact]
    public void GetUnusedAliases()
    {
        // arrange
        var gen = new SqlBuilder();
        var sql = gen.AppendAlias(a => a.WithSynonyms())
                     .AppendAlias(a => a.WithSynonyms())
                     .AppendAlias(a => a.WithSynonyms())
                     .AppendAlias(a => a.WithSynonyms())
                     .AppendAlias(a => a.WithSynonyms())
                     .ToSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetUnusedAliases()
                             .ToArray();

        // assert
        aliases.Length.ShouldBe(gen.IdSequence);
        Assert.All(aliases, alias => alias.Count.ShouldBe(0));
    }

    [Fact]
    public void GetUsage()
        => Record.Exception(() => BuildRepository().GetUsage(Per.DayOfWeek))
                 .ShouldBeNull();

    [Fact]
    public void GetYearsWithUsage()
        => Record.Exception(() => BuildRepository().GetYearsWithUsage())
                 .ShouldBeNull();

    [Fact]
    public void HydrateAlias()
        => Record.Exception(() => BuildRepository().HydrateAlias(new AliasQueryResult()))
                 .ShouldBeNull();

    #endregion
}