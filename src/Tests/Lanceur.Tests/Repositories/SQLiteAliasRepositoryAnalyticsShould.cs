using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Repositories;

/// <summary>
///     These tests are designed to detect SQL errors.
///     Consider them as a health check for SQL queries in AliasRepository.
/// </summary>
public class SQLiteAliasRepositoryQueryShouldBeValid : TestBase

{
    #region Constructors

    public SQLiteAliasRepositoryQueryShouldBeValid(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private SQLiteAliasRepository BuildRepository(string sql = null, IConnectionString connectionString = null)
    {
        var connection = BuildFreshDb(sql, connectionString);
        var scope = new DbSingleConnectionManager(connection);
        var log = Substitute.For<ILoggerFactory>();
        var mappingService = new MappingService();
        var service = new SQLiteAliasRepository(
            scope,
            log,
            mappingService,
            new DbActionFactory(mappingService, log)
        );
        return service;
    }

    [Fact] public void GetAdditionalParameter() => BuildRepository().GetAdditionalParameter([1, 2, 3]);

    [Fact]
    public void GetAliasesWithoutNotes()
    {
        // arrange
        var i = 0;
        const int count = 2;
        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        var sql = new SqlBuilder().AppendAlias(
                                      ++i,
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetAliasesWithoutNotes()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(i);
            foreach (var alias in aliases) alias.Count.Should().Be(2, $"alias {alias.Name} has usage");
        }
    }

    [Fact] public void GetAll() => BuildRepository().GetAll();

    [Fact] public void GetAllAliasWithAdditionalParameters() => BuildRepository().GetAllAliasWithAdditionalParameters();

    [Theory]
    [InlineData(@"c:\Not\A\Real\File.exe")]
    [InlineData(@"C:\NOT\A\REAL\FILE.EXE")]
    [InlineData(@"c:\not\a\real\file\")]
    [InlineData(@"C:\NOT\A\REAL\FILE\")]
    public void GetBrokenAliases(string fileName)
    {
        // arrange
        var i = 0;
        const int count = 2;
        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        var sql = new SqlBuilder().AppendAlias(
                                      ++i,
                                      $"{fileName}_{i}",
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      $"{fileName}_{i}",
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      $"{fileName}_{i}",
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      $"{fileName}_{i}",
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      $"{fileName}_{i}",
                                      props: new() { Count = count },
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetBrokenAliases()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(i);
            foreach (var alias in aliases) alias.Count.Should().Be(2, $"alias {alias.Name} has usage");
        }
    }

    [Fact] public void GetById() => BuildRepository().GetById(1);

    [Fact]
    public void GetDeletedAlias()
    {
        // arrange
        var i = 0;
        const int count = 2;
        var now = DateTime.Now.AddMinutes(-60);
        var date1 = now.AddDays(-1);
        var date2 = now.AddDays(-2);

        var sql = new SqlBuilder().AppendAlias(
                                      ++i,
                                      props: new(DeletedAt: now, Count: count),
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new(DeletedAt: now, Count: count),
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new(DeletedAt: now, Count: count),
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new(DeletedAt: now, Count: count),
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      props: new(DeletedAt: now, Count: count),
                                      cfg: a =>
                                      {
                                          a.WithUsage(date1, date2);
                                          a.WithSynonyms();
                                      }
                                  )
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDeletedAlias()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(i);
            foreach (var alias in aliases) alias.Count.Should().Be(2, $"alias {alias.Name} has usage");
        }
    }

    [Fact]
    public void GetDoubloons()
    {
        // arrange
        var i = 0;
        const int count = 2;
        const string name = "is_a_doubloon";
        var sql = new SqlBuilder().AppendAlias(
                                      ++i,
                                      name,
                                      name,
                                      new() { Count = count }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      name,
                                      name,
                                      new() { Count = count }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      name,
                                      name,
                                      new() { Count = count }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      name,
                                      name,
                                      new() { Count = count }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      name,
                                      name,
                                      new() { Count = count }
                                  )
                                  .ToString();
        var service = BuildRepository(sql);;

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(i);
            foreach (var alias in aliases) alias.Count.Should().Be(count);
        }
    }

    [Fact]
    public void GetExistingAliases()
    {
        // arrange
        var i = 0;
        var sql = new SqlBuilder().AppendAlias(++i, cfg: a => a.WithSynonyms("name_1"))
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms("name_1"))
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms("name_1"))
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms("name_1"))
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms("name_1"))
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        const string name = "name_1";
        var aliases = service.GetExistingAliases([name, "no_exist", "still_no_exist"], 1)
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(4);
            aliases[0].Should().Be(name);
        }
    }

    [Fact] public void GetExistingDeletedAliases() => BuildRepository().GetExistingDeletedAliases([], 0);

    [Fact] public void GetHiddenCounters() => BuildRepository().GetHiddenCounters();


    [Fact]
    public void GetInactiveAliases()
    {
        // arrange
        var i = 0;
        var date1 = DateTime.Now.AddMonths(-10);
        var date2 = DateTime.Now.AddMonths(-20);
        var sql = new SqlBuilder().AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(DateTime.Now);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetInactiveAliases(1)
                             .ToArray();

        // assert
        aliases.Should().HaveCount(4);
    }

    [Fact]
    public void GetRarelyUsedAliases()
    {
        // arrange
        var i = 0;
        var date1 = DateTime.Now.AddMonths(-10);
        var date2 = DateTime.Now.AddMonths(-20);
        var date3 = DateTime.Now.AddMonths(-30);
        var sql = new SqlBuilder().AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2, date3);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .AppendAlias(
                                      ++i,
                                      cfg: a =>
                                      {
                                          a.WithSynonyms();
                                          a.WithUsage(date1, date2);
                                      }
                                  )
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetRarelyUsedAliases(3)
                             .ToArray();

        // assert
        aliases.Should().HaveCount(4);
    }

    [Fact]
    public void GetUnusedAliases()
    {
        // arrange
        var i = 0;
        var sql = new SqlBuilder().AppendAlias(++i, cfg: a => a.WithSynonyms())
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms())
                                  .AppendAlias(++i, cfg: a =>  a.WithSynonyms())
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms())
                                  .AppendAlias(++i, cfg: a => a.WithSynonyms())
                                  .ToString();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetUnusedAliases()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(i);
            foreach (var alias in aliases) alias.Count.Should().Be(0);
        }
    }

    [Fact] public void GetUsage() => BuildRepository().GetUsage(Per.DayOfWeek);

    [Fact] public void GetYearsWithUsage() => BuildRepository().GetYearsWithUsage();

    [Fact] public void HydrateAlias() => BuildRepository().HydrateAlias(new());

    [Fact] public void HydrateMacro() => BuildRepository().HydrateMacro(new AliasQueryResult());

    #endregion
}