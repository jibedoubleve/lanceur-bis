using System.Reflection;
using AutoMapper;
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
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Utils;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic
{
    public class SearchShould : SQLiteTest
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
            var service = new SearchService(new StoreLoader());

            service.Stores.Should().HaveCount(5);
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
        public void ReturnNoResultMessageWhenMisconfiguredMacro()
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
            storeLoader.Load().Returns(new[] { new AliasStore(repository) });

            var asm = Assembly.GetExecutingAssembly();
            var service = new SearchService(storeLoader, new MacroManager(asm, repository: repository), thumbnailManager);

            // ACT
            var result = service.Search(new("z")).ToArray();

            // ASSERT
            using (new AssertionScope())
            {
                result.Should().HaveCountGreaterThan(0);
                result[0].Name.Should().Be("No result found");
            }
        }

        [Fact]
        public void ReturnResultWithExactMatchOnTop()
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

            var searchService = new SearchService(
                stores,
                macroManager,
                Substitute.For<IThumbnailManager>()
            );

            // ACT
            var result = searchService.Search(new(criterion))
                                      .ToArray();

            // ASSERT
            using (new AssertionScope())
            {
                result.Should().HaveCount(2);
                result.First().Name.Should().Be("u");
                result.First().Id.Should().Be(4000);
            }
        }

        [Fact]
        public void ReturnValues()
        {
            var macroManager = Substitute.For<IMacroManager>();
            var thumbnailManager = Substitute.For<IThumbnailManager>();
            var service = new SearchService(new DebugStoreLoader(), macroManager, thumbnailManager);
            var query = new Cmdline("code");

            var result = service.Search(query)
                                .ToArray();

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                result.ElementAt(0).IsResult.Should().BeFalse();
            }
        }

        #endregion Methods
    }
}