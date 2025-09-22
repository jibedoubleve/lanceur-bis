using System.Windows.Media.Animation;
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
        const int count = 2;
        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        var gen = new SqlGenerator();
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
                     .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetAliasesWithoutNotes()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(gen.IdSequence);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(2, $"alias {alias.Name} has usage");
                alias.LastUsedAt.Should().Be(date1);
            }
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
        const int count = 2;
        var date1 = DateTime.Now.AddDays(-1);
        var date2 = DateTime.Now.AddDays(-2);

        var gen = new SqlGenerator();
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
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetBrokenAliases()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(gen.IdSequence);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(2, $"alias {alias.Name} has usage");
                alias.LastUsedAt.Should().Be(date1);
            }
        }
    }

    [Fact] public void GetById() => BuildRepository().GetById(1);

    [Fact]
    public void GetDeletedAlias()
    {
        // arrange
        const int count = 2;
        var now = DateTime.Now.AddMinutes(-60);
        var date1 = now.AddDays(-1);
        var date2 = now.AddDays(-2);

        var gen = new SqlGenerator();
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
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDeletedAlias()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(gen.IdSequence);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(2, $"alias {alias.Name} has usage");
                alias.LastUsedAt.Should().Be(date1);
            }
        }
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
        var gen = new SqlGenerator();
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
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(gen.IdSequence);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(count);
                alias.LastUsedAt.Should().Be(date1);
            }
        }
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
        var gen = new SqlGenerator();
        var sql = gen.AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(script1)
                                        .WithSynonyms(name)
                                        .WithUsage(date1, date2))
                    .AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(script1)
                                        .WithSynonyms(name)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name))
                    .AppendAlias(a => a.WithFileName(name)
                                        .WithArguments(name)
                                        .WithCount(count)
                                        .WithLuaScript(script1)
                                        .WithUsage(date1, date2)
                                        .WithSynonyms(name))
                    .AppendAlias(a => a.WithFileName(name)
                                       .WithArguments(name)
                                       .WithCount(count)
                                       .WithLuaScript($"{Guid.NewGuid()}")
                                       .WithUsage(date1, date2)
                                       .WithSynonyms(name))
                    .AppendAlias(a => a.WithFileName(name)
                                       .WithArguments(name)
                                       .WithCount(count)
                                       .WithLuaScript($"{Guid.NewGuid()}")
                                       .WithUsage(date1, date2)
                                       .WithSynonyms(name)
                                    )
                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(3);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(count);
                alias.LastUsedAt.Should().Be(date1);
            }
        }
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
        var sql = new SqlGenerator().AppendAlias(a => a.WithFileName(name)
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
                                                       .WithLuaScript($"{Guid.NewGuid()}")
                                                       .WithUsage(date1, date2)
                                                       .WithSynonyms(name)
                                    )
                                    .AppendAlias(a => a.WithFileName(name)
                                                       .WithArguments(name)
                                                       .WithCount(count)
                                                       .WithLuaScript($"{Guid.NewGuid()}")
                                                       .WithUsage(date1, date2)
                                                       .WithSynonyms(name)
                                    )
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetDoubloons()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(2);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(count);
                alias.LastUsedAt.Should().Be(date1);
            }
        }
    }

    [Fact]
    public void GetExistingAliases()
    {
        // arrange
        var sql = new SqlGenerator().AppendAlias(a => a.WithSynonyms("name_1"))
                                    .AppendAlias(a => a.WithSynonyms("name_1"))
                                    .AppendAlias(a => a.WithSynonyms("name_1"))
                                    .AppendAlias(a => a.WithSynonyms("name_1"))
                                    .AppendAlias(a => a.WithSynonyms("name_1"))
                                    .GenerateSql();
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
        var date1 = DateTime.Now.AddMonths(-10);
        var date2 = DateTime.Now.AddMonths(-20);
        var sql = new SqlGenerator().AppendAlias(a =>
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
                                    .AppendAlias(
                                        a =>
                                            a.WithSynonyms()
                                             .WithUsage(date1, date2)
                                    )
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetInactiveAliases(1)
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(4);
            foreach (var alias in aliases)
            {
                alias.Count.Should().Be(2);
                alias.LastUsedAt.Should().Be(date1);
            }
        }
    }

    [Fact]
    public void GetRarelyUsedAliases()
    {
        // arrange
        var date1 = DateTime.Now.AddMonths(-10);
        var date2 = DateTime.Now.AddMonths(-20);
        var date3 = DateTime.Now.AddMonths(-30);
        var sql = new SqlGenerator().AppendAlias(a =>
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
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetRarelyUsedAliases(3)
                             .ToArray();

        // assert
        aliases.Should().HaveCount(4);
        foreach (var alias in aliases)
        {
            alias.Count.Should().Be(2);
            alias.LastUsedAt.Should().Be(date1);
        }
    }

    [Fact]
    public void GetUnusedAliases()
    {
        // arrange
        var gen = new SqlGenerator();
        var sql = gen.AppendAlias(a => a.WithSynonyms())
                                    .AppendAlias(a => a.WithSynonyms())
                                    .AppendAlias(a => a.WithSynonyms())
                                    .AppendAlias(a => a.WithSynonyms())
                                    .AppendAlias(a => a.WithSynonyms())
                                    .GenerateSql();
        var service = BuildRepository(sql);

        // act
        var aliases = service.GetUnusedAliases()
                             .ToArray();

        // assert
        using (new AssertionScope())
        {
            aliases.Should().HaveCount(gen.IdSequence);
            foreach (var alias in aliases) alias.Count.Should().Be(0);
        }
    }

    [Fact] public void GetUsage() => BuildRepository().GetUsage(Per.DayOfWeek);

    [Fact] public void GetYearsWithUsage() => BuildRepository().GetYearsWithUsage();

    [Fact] public void HydrateAlias() => BuildRepository().HydrateAlias(new());

    [Fact] public void HydrateMacro() => BuildRepository().HydrateMacro(new AliasQueryResult());

    #endregion
}