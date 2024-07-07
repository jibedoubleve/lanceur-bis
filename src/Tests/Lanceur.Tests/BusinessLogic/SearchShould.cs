﻿using System.Reflection;
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
using Lanceur.Tests.Logging;
using Lanceur.Tests.SQL;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Utils;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Splat;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic
{
    public class SearchShould : TestBase
    {
        #region Fields

        private const string SqlCreateAlias = @"
                insert into alias (id, file_name, arguments, id_session) values (1000, '@multi@', '@alias2@@alias3', 1);
                insert into alias_name (id, id_alias, name) values (1000, 1000, 'alias1');

                insert into alias (id, file_name, arguments,id_session) values (2000, 'arg', 'c:\dummy\dummy.exe', 1);
                insert into alias_name (id, id_alias, name) values (2000, 2000, 'alias2');

                insert into alias (id, file_name, arguments,id_session) values (3000, 'arg', 'c:\dummy\dummy.exe', 1);
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
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var orchestrator = Substitute.For<ISearchServiceOrchestrator>();
            
            var service = new SearchService(new StoreLoader(loggerFactory, orchestrator), loggerFactory: loggerFactory);

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
            const string sql = SqlCreateAlias
                               + @"
                insert into alias (id, file_name, id_session) values (4000, '@zzzz@', 1);
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
            var service = new SearchService(storeLoader, new MacroManager(asm, repository: repository), thumbnailManager, orchestrator: orchestrator);

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
            insert into alias (id, file_name, arguments, id_session) values (1000, 'un', '@alias2@@alias3', 1);
            insert into alias_name (id, id_alias, name) values (1001, 1000, 'un');

            insert into alias (id, file_name, arguments, id_session) values (2000, 'deux', '@alias2@@alias3', 1);
            insert into alias_name (id, id_alias, name) values (1002, 2000, 'deux');

            insert into alias (id, file_name, arguments, id_session) values (3000, 'trois', '@alias2@@alias3', 1);
            insert into alias_name (id, id_alias, name) values (1003, 3000, 'trois');
            --
            insert into alias (id, file_name, arguments, id_session) values (4000, 'u', '@alias2@@alias3', 1);
            insert into alias_name (id, id_alias, name) values (1004, 4000, 'u');
            --
            insert into alias_usage (id_alias, id_session, time_stamp) values (1000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, id_session, time_stamp) values (1000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, id_session, time_stamp) values (1000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            ---
            insert into alias_usage (id_alias, id_session, time_stamp) values (2000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, id_session, time_stamp) values (2000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, id_session, time_stamp) values (2000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            ---
            insert into alias_usage (id_alias, id_session, time_stamp) values (3000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, id_session, time_stamp) values (3000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
            insert into alias_usage (id_alias, id_session, time_stamp) values (3000, 1, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
         ";

            // ARRANGE
            using var db = BuildFreshDb(sql);
            using var conn = new DbSingleConnectionManager(db);

            const string criterion = "u";
            var repository = new SQLiteRepository(conn, _testLoggerFactory, converter);
            var stores = Substitute.For<IStoreLoader>();
            stores.Load().Returns(new[] { new AliasStore(repository) });

            var results = repository.Search(criterion, 1).ToList();
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
            QueryResult alias = new AliasQueryResult
            {
                Id = 1,
                Name = "a"
            };

            var repository = new SQLiteRepository(connectionMgr, logger, converter);

            OutputHelper.Act();
            repository.SetUsage(alias);

            OutputHelper.Assert();
            const string sqlCount = "select count(*) from alias_argument";

            connectionMgr.WithinTransaction(x => x.Connection.ExecuteScalar<int>(sqlCount))
                         .Should().Be(3);
        }

        #endregion Methods
    }
}