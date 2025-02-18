using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Utils;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.Macros;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic;

public class MacroServiceShould : TestBase
{
    #region Constructors

    public MacroServiceShould(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Fact]
    public void BeCastedToSelfExecutableQueryResult()
    {
        // ARRANGE
        var srcNamespace = typeof(MultiMacro).Namespace!;
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
                var sut = Activator.CreateInstance(type, sp);
                sut.Should()
                   .BeAssignableTo(typeof(SelfExecutableQueryResult));

                sut.Should()
                   .BeAssignableTo(typeof(MacroQueryResult));
            }
        }
    }

    [Theory]
    [InlineData("multi", true)]
    [InlineData("MULTI", true)]
    [InlineData("macro", false)]
    [InlineData("MACRO", false)]
    [InlineData("calendar", false)]
    [InlineData("CALENDAR", false)]
    public void BeCompositeAsExpected(string macro, bool expected)
    {
        var alias = new AliasQueryResult { FileName = $"@{macro}@" };

        alias.IsComposite().Should().Be(expected);
    }

    [Theory]
    [InlineData("init", "a@z@e@r@t@t")]
    [InlineData("home", "azerty")]
    [InlineData("some", "a z e r t y")]
    public async Task BeExecutable(string name, string parameters)
    {
        var serviceProvider = new ServiceCollection().AddMockSingleton<ILogger<MacroService>>()
                                                     .AddMockSingleton<IAliasRepository>()
                                                     .AddMockSingleton<ILoggerFactory>()
                                                     .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                                                     .AddSingleton<MacroService>()
                                                     .BuildServiceProvider();
        var macroMgr = serviceProvider.GetService<MacroService>();
        var macro = new MultiMacroTest(serviceProvider) { Parameters = parameters };
        var handler = (SelfExecutableQueryResult)macroMgr.ExpandMacroAlias(macro);

        var cmdline = new Cmdline(name, parameters);
        var results = (await handler.ExecuteAsync(cmdline))
            .ToArray();

        results.ElementAt(0).Name.Should().Be(name);
        results.ElementAt(0).Description.Should().Be(parameters);
    }

    [Fact]
    public void BeExecutableQueryResult()
    {
        var serviceProvider = new ServiceCollection().AddMockSingleton<IAliasRepository>()
                                                     .AddMockSingleton<ILogger<MacroService>>()
                                                     .AddMockSingleton<ILoggerFactory>()
                                                     .AddSingleton(new AssemblySource { MacroSource = Assembly.GetExecutingAssembly() })
                                                     .AddSingleton<MacroService>()
                                                     .AddSingleton<MultiMacroTest>()
                                                     .BuildServiceProvider();
        var macroMgr = serviceProvider.GetService<MacroService>();
        var macro = new MultiMacroTest(serviceProvider);
        var result = macroMgr.ExpandMacroAlias(macro);

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
        var serviceProvider = new ServiceCollection().AddSingleton(new AssemblySource { MacroSource = Assembly.GetAssembly(typeof(MultiMacro))! })
                                                     .AddSingleton<ILoggerFactory, LoggerFactory>()
                                                     .AddMockSingleton<ILogger<MacroService>>()
                                                     .AddMockSingleton<IAliasRepository>()
                                                     .AddSingleton<MacroService>()
                                                     .BuildServiceProvider();
        var manager = serviceProvider.GetService<MacroService>();
        manager.MacroCount.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
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
        QueryResult[] queryResults = [
            new AliasQueryResult { Name = "macro_1", FileName = "@multi@" }, 
            new AliasQueryResult { Name = "macro_2", FileName = "@multi@" }, 
            new AliasQueryResult { Name = "macro_3", FileName = "@multi@" }
        ];

        var serviceProvider = new ServiceCollection().AddMockSingleton<ILogger<MacroService>>()
                                                     .AddMockSingleton<IAliasRepository>()
                                                     .AddMockSingleton<ILoggerFactory>()
                                                     .AddSingleton(new AssemblySource { MacroSource = Assembly.GetAssembly(typeof(MultiMacro)) })
                                                     .AddSingleton<MacroService>()
                                                     .BuildServiceProvider();

        var output = serviceProvider.GetService<MacroService>()
                                    .ExpandMacroAlias(queryResults)
                                    .ToArray();

        using (new AssertionScope())
        {
            output.GetDoubloons().Should().HaveCount(0);
            output.Should().HaveCount(3);
        }
    }

    [Theory]
    [InlineData("un")]
    [InlineData("deux")]
    [InlineData("trois")]
    public void RecogniseMacro(string macro)
    {
        var query = new AliasQueryResult { FileName = $"@{macro}@" };

        query.IsMacro().Should().BeTrue();
    }

    #endregion

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

        #endregion

        #region Methods

        private static IMappingService GetConversionService()
        {
            var cfg = new MapperConfiguration(c => { c.CreateMap<AliasQueryResult, CompositeAliasQueryResult>(); });
            return new AutoMapperMappingService();
        }

        public static IAliasRepository GetDataService(IDbConnection db)
        {
            var log = Substitute.For<ILoggerFactory>();
            var conv = GetConversionService();
            var service = new SQLiteAliasRepository(
                new DbSingleConnectionManager(db),
                log,
                conv,
                new DbActionFactory(new AutoMapperMappingService(), log)
            );
            return service;
        }

        #endregion
    }

    #endregion Classes
}