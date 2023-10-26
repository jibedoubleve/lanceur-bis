using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Logging;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Utils;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic
{
    public class SearchShould : SQLiteTest
    {
        #region Fields

        private readonly IAppLoggerFactory _testLoggerFactory;

        private string SQL_CreateAlias = @"
                insert into alias (id, file_name, arguments, id_session) values (1000, '@multi@', '@alias2@@alias3', 1);
                insert into alias_name (id, id_alias, name) values (1000, 1000, 'alias1');

                insert into alias (id, file_name, arguments,id_session) values (2000, 'arg', 'c:\dummy\dummy.exe', 1);
                insert into alias_name (id, id_alias, name) values (2000, 2000, 'alias2');

                insert into alias (id, file_name, arguments,id_session) values (3000, 'arg', 'c:\dummy\dummy.exe', 1);
                insert into alias_name (id, id_alias, name) values (3000, 3000, 'alias3')";

        #endregion Fields

        #region Constructors

        public SearchShould(ITestOutputHelper output)
        {
            _testLoggerFactory = new XUnitLoggerFactory(output);
        }

        #endregion Constructors

        #region Methods

        private static SearchService BuildSearchService(IStoreLoader loader = null)
        {
            loader ??= new DebugStoreLoader();
            var service = new SearchService(loader);
            return service;
        }

        [Fact]
        public void NOT_HaveNullParameters()
        {
            // arrange
            var converter = Substitute.For<IConvertionService>();
            var sql = SQL_CreateAlias;
            using var db = BuildFreshDb(sql);
            using var scope = new SQLiteDbConnectionManager(db);

            var action = new SQLiteRepository(scope, _testLoggerFactory, converter);

            // act
            var results = action.GetAll();
            var parameters = results.Select(c => c.Parameters);

            //assert
            parameters.Should().NotContain((string)null);

        }

        [Fact]
        public void HaveStores()
        {
            SearchService service = BuildSearchService(new StoreLoader());

            service.Stores.Should().HaveCount(5);
        }

        [Fact]
        public void ReturnValues()
        {
            var service = BuildSearchService();
            var query = new Cmdline("code");

            var result = service.Search(query);

            result.Should().HaveCount(1);
            result.ElementAt(0).IsResult.Should().BeFalse();
        }

        #endregion Methods
    }
}