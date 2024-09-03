using System.Reflection;
using AutoMapper;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Stores;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Splat;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic;

public class SearchShould : TestBase
{
    #region Fields

    private const string SqlCreateAlias = @"
                insert into alias (id, file_name, arguments) values (1000, '@multi@', '@alias2@@alias3');
                insert into alias_name (id, id_alias, name) values (1000, 1000, 'alias1');

                insert into alias (id, file_name, arguments) values (2000, 'arg', 'c:\dummy\dummy.exe');
                insert into alias_name (id, id_alias, name) values (2000, 2000, 'alias2');

                insert into alias (id, file_name, arguments) values (3000, 'arg', 'c:\dummy\dummy.exe');
                insert into alias_name (id, id_alias, name) values (3000, 3000, 'alias3');";

    private readonly ILoggerFactory _testLoggerFactory;

    #endregion Fields

    #region Constructors

    public SearchShould(ITestOutputHelper output) : base(output)
    {
        _testLoggerFactory = new MicrosoftLoggingLoggerFactory(output);
    }

    #endregion Constructors

    #region Methods

    private static IConversionService GetConversionService()
    {
        var cfg = new MapperConfiguration(c => { c.CreateMap<AliasQueryResult, CompositeAliasQueryResult>(); });
        return new AutoMapperConverter(new Mapper(cfg));
    }

    [Fact]
    public void HaveStores()
    {
        var logger = Substitute.For<ILogger<StoreLoader>>();
        var orchestrator = Substitute.For<ISearchServiceOrchestrator>();
        var serviceProvider
            = new ServiceCollection().AddTransient(_ => Substitute.For<ILoggerFactory>())
                                     .AddTransient(_ => Substitute.For<ISearchServiceOrchestrator>())
                                     .BuildServiceProvider();

        var service = new SearchService(new StoreLoader(logger, orchestrator, serviceProvider));

        service.Stores.Should().HaveCountGreaterThan(5);
    }

    [Fact]
    public void NOT_HaveNullParameters()
    {
        // arrange
        var converter = GetConversionService();
        using var db = BuildFreshDb(SqlCreateAlias);
        using var conn = new DbSingleConnectionManager(db);

        var repository = new SQLiteRepository(conn, _testLoggerFactory, converter);

        // act
        var results = repository.GetAll();
        var parameters = results.Select(c => c.Parameters);

        //assert
        parameters.Should().NotContain((string)null);
    }

    [Fact]
    public async Task ReturnNoResultMessageWhenMisconfiguredMacro()
    {
        // ARRANGE
        const string sql = SqlCreateAlias +
                           @"
                insert into alias (id, file_name) values (4000, '@zzzz@');
                insert into alias_name (id, id_alias, name) values (4000, 4000, 'zz');";

        using var db = BuildFreshDb(sql);
        using var conn = new DbSingleConnectionManager(db);

        var thumbnailManager = Substitute.For<IThumbnailManager>();
        var converter = Substitute.For<IConversionService>();
        var repository = new SQLiteRepository(conn, _testLoggerFactory, converter);
        var storeLoader = Substitute.For<IStoreLoader>();
        var orchestrator = Substitute.For<ISearchServiceOrchestrator>();
        storeLoader.Load().Returns(new[] { new AliasStore(repository) });

        var asm = Assembly.GetExecutingAssembly();
        ILogger<MacroManager> logger = new TestOutputHelperDecoratorForMicrosoftLogging<MacroManager>(OutputHelper);
        var service = new SearchService(storeLoader, new MacroManager(asm, logger, repository, new ServiceCollection().BuildServiceProvider()), thumbnailManager, orchestrator: orchestrator);

        // ACT
        var result = (await service.SearchAsync(new("z"))).ToArray();

        // ASSERT
        using (new AssertionScope())
        {
            result.Should().HaveCountGreaterThan(0);
            result[0].Name.Should().Be("No result found");
        }
    }

    [Fact]
    public async Task ReturnResultWithExactMatchOnTop()
    {
        var dt = DateTime.Now;
        var converter = Substitute.For<IConversionService>();
        var sql = @$"
            insert into alias (id, file_name, arguments) values (1000, 'un', '@alias2@@alias3');
            insert into alias_name (id, id_alias, name) values (1001, 1000, 'un');

            insert into alias (id, file_name, arguments) values (2000, 'deux', '@alias2@@alias3');
            insert into alias_name (id, id_alias, name) values (1002, 2000, 'deux');

            insert into alias (id, file_name, arguments) values (3000, 'trois', '@alias2@@alias3');
            insert into alias_name (id, id_alias, name) values (1003, 3000, 'trois');
            --
            insert into alias (id, file_name, arguments) values (4000, 'u', '@alias2@@alias3');
            insert into alias_name (id, id_alias, name) values (1004, 4000, 'u');
            --
            insert into alias_usage (id_alias, time_stamp) values (1000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, time_stamp) values (1000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, time_stamp) values (1000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            ---
            insert into alias_usage (id_alias, time_stamp) values (2000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, time_stamp) values (2000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, time_stamp) values (2000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            ---
            insert into alias_usage (id_alias, time_stamp) values (3000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, time_stamp) values (3000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, time_stamp) values (3000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
         ";

        // ARRANGE
        using var db = BuildFreshDb(sql);
        using var conn = new DbSingleConnectionManager(db);

        const string criterion = "u";
        var repository = new SQLiteRepository(conn, _testLoggerFactory, converter);
        var stores = Substitute.For<IStoreLoader>();
        stores.Load().Returns(new[] { new AliasStore(repository) });

        var results = repository.Search(criterion).ToList();
        var macroManager = Substitute.For<IMacroManager>();
        macroManager.Handle(Arg.Any<QueryResult[]>())
                    .Returns(results);

        var orchestrator = Substitute.For<ISearchServiceOrchestrator>();
        orchestrator.IsAlive(Arg.Any<ISearchService>(), Arg.Any<Cmdline>())
                    .Returns(true);

        var searchService = new SearchService(
            stores,
            macroManager,
            Substitute.For<IThumbnailManager>(),
            orchestrator: orchestrator
        );

        // ACT
        var result = (await searchService.SearchAsync(new(criterion))).ToArray();

        // ASSERT
        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("u");
            result.First().Id.Should().Be(4000);
        }
    }

    [Fact]
    public async Task ReturnValues()
    {
        var macroManager = Substitute.For<IMacroManager>();
        var thumbnailManager = Substitute.For<IThumbnailManager>();
        var query = new Cmdline("code");
        var orchestrator = Substitute.For<ISearchServiceOrchestrator>();
        orchestrator.IsAlive(Arg.Any<ISearchService>(), Arg.Any<Cmdline>())
                    .Returns(true);
        var service = new SearchService(new DebugStoreLoader(), macroManager, thumbnailManager, orchestrator: orchestrator);

        var result = (await service.SearchAsync(query))
            .ToArray();

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result.ElementAt(0).IsResult.Should().BeFalse();
        }
    }

    [Fact]
    public void SetUsageDoesNotResetAdditionalParameters()
    {
        OutputHelper.Arrange();
        var sql = new SqlBuilder().AppendAlias(1)
                                  .AppendSynonyms(1, "a")
                                  .AppendArgument(1)
                                  .AppendArgument(1)
                                  .AppendArgument(1)
                                  .ToString();

        var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));
        var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
        var converter = Substitute.For<IConversionService>();
        QueryResult alias = new AliasQueryResult { Id = 1, Name = "a" };

        var repository = new SQLiteRepository(connectionMgr, logger, converter);

        OutputHelper.Act();
        repository.SetUsage(alias);

        OutputHelper.Assert();
        const string sqlCount = "select count(*) from alias_argument";

        connectionMgr.WithinTransaction(x => x.Connection.ExecuteScalar<int>(sqlCount))
                     .Should()
                     .Be(3);
    }

    [Fact]
    public void NotSetUsageWhenCounterIsNegative()
    {
        /*
         * Create a new alias with usage set to -1
         * Execute it 4 times
         * Check counter is still -1
         */
        OutputHelper.Arrange();
        var sql = new SqlBuilder().AppendAlias(1)
                                  .AppendSynonyms(1, "a")
                                  .ToString();
        var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));
        var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
        var converter = Substitute.For<IConversionService>();
        QueryResult alias = new AliasQueryResult { Id = 1, Name = "a", Count = -1 };

        var repository = new SQLiteRepository(connectionMgr, logger, converter);

        OutputHelper.Act();

        for (var i = 0; i < 5; i++) repository.SetUsage(alias);

        OutputHelper.Assert();
        const string sqlCount = "select count(*) from alias_usage where id_alias = 1";

        connectionMgr.WithinTransaction(x => x.Connection.ExecuteScalar<int>(sqlCount))
                     .Should()
                     .Be(0);
    }

    #endregion Methods
}