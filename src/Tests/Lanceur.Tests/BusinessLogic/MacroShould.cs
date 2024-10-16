using System.Data;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.Utils;
using Lanceur.Tests.SQLite;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Data.SQLite;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lanceur.Infra.Macros;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.Tooling.Macros;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic;

public class MacroShould : TestBase
{
    #region Constructors

    public MacroShould(ITestOutputHelper output) : base(output) { }

    #endregion Constructors

    #region Methods

    [Fact]
    public void BeCastedToSelfExecutableQueryResult()
    {
        // ARRANGE
        var srcNamespace = typeof(MultiMacro).Namespace;
        var asm = Assembly.GetAssembly(typeof(MultiMacro));

        var types = asm!.GetTypes()
                        .Where(type => { return type.Namespace != null && type.Namespace.StartsWith(srcNamespace); })
                        // I'll exclude all the anonymous class from the query
                        .Where(type => !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
                        .ToArray();
        // ACT
        // ASSERT
        types.Length.Should().BeGreaterThan(0, "there are preconfigured macros");
        var sp = new ServiceCollection().BuildServiceProvider();
        using (new AssertionScope())
        {
            foreach (var type in types)
            {
                OutputHelper.WriteLine($"Checking '{type.FullName}'");
                var sut = Activator.CreateInstance(type, [sp]);
                sut.Should()
                   .BeAssignableTo(typeof(SelfExecutableQueryResult));

                sut.Should()
                   .BeAssignableTo(typeof(MacroQueryResult));
            }
        }
    }

    [Theory, InlineData("multi", true), InlineData("MULTI", true), InlineData("macro", false), InlineData("MACRO", false), InlineData("calendar", false), InlineData("CALENDAR", false)]
    public void BeCompositeAsExpected(string macro, bool expected)
    {
        var alias = new AliasQueryResult
        {
            FileName = $"@{macro}@"
        };

        alias.IsComposite().Should().Be(expected);
    }

    [Theory, InlineData("init", "a@z@e@r@t@t"), InlineData("home", "azerty"), InlineData("some", "a z e r t y")]
    public async Task BeExecutable(string name, string parameters)
    {
        var logger = Substitute.For<ILogger<MacroManager>>();
        var repository = Substitute.For<IDbRepository>();

        var asm = Assembly.GetExecutingAssembly();
        var macroMgr = new MacroManager(asm, logger, repository, new ServiceCollection().BuildServiceProvider());
        var macro = new MultiMacroTest(parameters);
        var handler = (SelfExecutableQueryResult)macroMgr.Handle(macro);

        var cmdline = new Cmdline(name, parameters);
        var results = (await handler.ExecuteAsync(cmdline))
            .ToArray();

        results.ElementAt(0).Name.Should().Be(name);
        results.ElementAt(0).Description.Should().Be(parameters);
    }

    [Fact]
    public void BeExecutableQueryResult()
    {
        var logger = Substitute.For<ILogger<MacroManager>>();
        var repository = Substitute.For<IDbRepository>();

        var asm = Assembly.GetExecutingAssembly();
        var macroMgr = new MacroManager(asm, logger, repository, new ServiceCollection().BuildServiceProvider());
        var macro = new MultiMacroTest();
        var result = macroMgr.Handle(macro);

        result.Should().BeAssignableTo<SelfExecutableQueryResult>();
    }

    [Fact]
    public void BeMacroComposite()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db);

        // Act
        var results = service.Search("alias1");

        // Assert
        (results.ElementAt(0) as CompositeAliasQueryResult).Should().NotBeNull();
    }

    [Fact]
    public void BeRecognisedAsAMacro()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db);

        //Act
        var results = service.Search("alias1")
                             .ToArray();

        //Assert
        using (new AssertionScope())
        {
            results.Should().HaveCount(1);
            results.ElementAt(0).IsMacro().Should().BeTrue();
        }
    }

    [Fact]
    public void HaveDefaultMacro()
    {
        var asm = Assembly.GetAssembly(typeof(MultiMacro));
        var logger = Substitute.For<ILogger<MacroManager>>();
        var repository = Substitute.For<IDbRepository>();
        var manager = new MacroManager(asm, logger, repository, new ServiceCollection().BuildServiceProvider());

        manager.MacroCount.Should().BeGreaterThan(0);
    }

    [Theory, InlineData(0, 1), InlineData(1, 2)]
    public void HaveDelayOnFirstElement(int index, int delay)
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db);

        // Act
        var results = service.Search("alias1");

        // Assert
        using (new AssertionScope())
        {
            var composite = results.ElementAt(0) as CompositeAliasQueryResult;
            composite.Should().NotBeNull();
            composite?.Aliases.ElementAt(index).Delay.Should().Be(delay);
        }
    }

    [Fact]
    public void HaveExpectedMacroName()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db);

        //Act
        var results = service.Search("alias1");

        //Assert
        results.ElementAt(0).GetMacroName().Should().Be("MULTI");
    }

    [Fact]
    public void NotBeRecognisedAsAMacro()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db);

        // Act
        var results = service.Search("alias2")
                             .ToArray();

        // Assert
        using (new AssertionScope())
        {
            results.Should().HaveCount(1);
            results.ElementAt(0).IsMacro().Should().BeFalse();
        }
    }

    [Fact]
    public void NotHaveDoubloonsWhenMacroUsedMultipleTimes()
    {
        var queryResults = new AliasQueryResult[]
        {
            new() { Name = "macro_1", FileName = "@multi@" },
            new() { Name = "macro_2", FileName = "@multi@" },
            new() { Name = "macro_3", FileName = "@multi@" }
        };

        var logger = Substitute.For<ILogger<MacroManager>>();
        var repository = Substitute.For<IDbRepository>();
        var asm = Assembly.GetExecutingAssembly();
        var manager = new MacroManager(asm, logger, repository, new ServiceCollection().BuildServiceProvider());

        var output = manager.Handle(queryResults)
                            .ToArray();

        using (new AssertionScope())
        {
            output.GetDoubloons().Should().HaveCount(0);
            output.Should().HaveCount(3);
        }
    }

    [Theory, InlineData("un"), InlineData("deux"), InlineData("trois")]
    public void RecogniseMacro(string macro)
    {
        var query = new AliasQueryResult()
        {
            FileName = $"@{macro}@"
        };

        query.IsMacro().Should().BeTrue();
    }

    #endregion Methods

    #region Classes

    private static class Cfg
    {
        #region Fields

        public const string SqlForAliases = @"
                insert into alias (id, file_name, arguments) values (1000, '@multi@', '@alias2@@alias3');
                insert into alias_name (id, id_alias, name) values (1000, 1000, 'alias1');

                insert into alias (id, file_name) values (2000, 'c:\dummy\dummy.exe');
                insert into alias_name (id, id_alias, name) values (2000, 2000, 'alias2');

                insert into alias (id, file_name) values (3000, 'c:\dummy\dummy.exe');
                insert into alias_name (id, id_alias, name) values (3000, 3000, 'alias3');";

        #endregion Fields

        #region Methods

        private static IMappingService GetConversionService()
        {
            var cfg = new MapperConfiguration(c => { c.CreateMap<AliasQueryResult, CompositeAliasQueryResult>(); });
            return new AutoMapperConverter(new Mapper(cfg));
        }

        public static IDbRepository GetDataService(IDbConnection db)
        {
            var log = Substitute.For<ILoggerFactory>();
            var conv = GetConversionService();
            var service = new SQLiteRepository(new DbSingleConnectionManager(db), log, conv);
            return service;
        }

        #endregion Methods
    }

    #endregion Classes
}