using System.Data;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.SQLite;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Lanceur.Infra.SQLite.DataAccess;
using Xunit;
using Xunit.Abstractions;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Tests.BusinessLogic
{
    public class AliasManagementShould : TestBase
    {
        #region Constructors

        public AliasManagementShould(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        #endregion Constructors

        #region Methods

        private static AliasQueryResult BuildAlias(string name = null, RunAs runAs = RunAs.CurrentUser)
        {
            name ??= Guid.NewGuid().ToString();
            return new()
            {
                Name = name,
                Synonyms = name,
                Parameters = string.Empty,
                FileName = @"C:\Users\jibedoubleve\AppData\Local\Programs\beekeeper-studio\Beekeeper Studioeuh.exe",
                RunAs = runAs,
                StartMode = StartMode.Default,
                WorkingDirectory = string.Empty,
                Icon = null,
            };
        }

        private static AliasDbAction BuildAliasDbAction(IDbConnection connection)
        {
            var scope = new DbSingleConnectionManager(connection);
            var log = Substitute.For<ILoggerFactory>();
            var action = new AliasDbAction(scope, log);
            return action;
        }

        private static SQLiteRepository BuildDataService(IDbConnection connection)
        {
            var scope = new DbSingleConnectionManager(connection);
            var log = Substitute.For<ILoggerFactory>();
            var conv = Substitute.For<IConversionService>();
            var service = new SQLiteRepository(scope, log, conv);
            return service;
        }

        [Fact]
        public void CreateAlias()
        {
            // ARRANGE
            const string sql = @"
                insert into alias(id) values (1001);
                insert into alias(id) values (1002);
                insert into alias(id) values (1003);

                insert into alias_name(id, name, id_alias) values (1000, 'noname', 1001);
                insert into alias_name(id, name, id_alias) values (2000, 'noname', 1002);
                insert into alias_name(id, name, id_alias) values (3000, 'noname', 1003);";

            var connection = BuildFreshDb(sql);
            var action = BuildAliasDbAction(connection);

            var alias1 = BuildAlias();
            var alias2 = BuildAlias();
            var alias3 = BuildAlias();

            // ACT
            action.Create(ref alias1);
            action.Create(ref alias2);
            action.Create(ref alias3);

            //ASSERT
            using (new AssertionScope())
            {
                alias1.Id.Should().Be(1004);
                alias2.Id.Should().Be(1005);
                alias3.Id.Should().Be(1006);

                const string sql2 = "select count(*) from alias";
                const string sql3 = "select count(*) from alias_name";

                connection.ExecuteScalar<int>(sql2).Should().Be(6);
                connection.ExecuteScalar<int>(sql3).Should().Be(6);
            }
        }

        [Fact]
        public void CreateAliasInvisibleUsingService()
        {
            // ARRANGE
            var aliasName = Guid.NewGuid().ToString();

            var connection = BuildFreshDb();
            var action = BuildAliasDbAction(connection);

            QueryResult alias1 = new AliasQueryResult { Name = aliasName };

            // ACT
            action.CreateInvisible(ref alias1);

            var sut = action.GetExact(aliasName, includeHidden: true);

            //ASSERT
            sut.Should().NotBeNull();
            sut.IsHidden.Should().Be(true);
        }

        [Fact]
        public void CreateAliasUsingService()
        {
            // ARRANGE
            var sql = @"
                insert into alias(id) values (1001);
                insert into alias(id) values (1002);
                insert into alias(id) values (1003);

                insert into alias_name(id, name, id_alias) values (1000, 'noname', 1001);
                insert into alias_name(id, name, id_alias) values (2000, 'noname', 1002);
                insert into alias_name(id, name, id_alias) values (3000, 'noname', 1003);";

            var connection = BuildFreshDb(sql);
            var service = BuildDataService(connection);

            const string name1 = "albert";
            const string name2 = "norbert";
            const string name3 = "philibert";

            var alias1 = BuildAlias(name1);
            var alias2 = BuildAlias(name2);
            var alias3 = BuildAlias(name3);

            // ACT
            service.SaveOrUpdate(ref alias1);
            service.SaveOrUpdate(ref alias2);
            service.SaveOrUpdate(ref alias3);

            //ASSERT
            using (new AssertionScope())
            {
                var sql2 = "select count(*) from alias;";
                var sql3 = "select count(*) from alias_name;";

                alias1.Name.Should().Be(name1);
                alias2.Name.Should().Be(name2);
                alias3.Name.Should().Be(name3);

                connection.ExecuteScalar<int>(sql2).Should().Be(6);
                connection.ExecuteScalar<int>(sql3).Should().Be(6);
            }
        }

        [Fact]
        public void CreateAliasVisibleByDefaultUsingService()
        {
            // ARRANGE
            var aliasName = Guid.NewGuid().ToString();

            var connection = BuildFreshDb();
            var service = BuildDataService(connection);

            var alias1 = BuildAlias(aliasName);

            // ACT
            service.SaveOrUpdate(ref alias1);

            var sut = service.GetExact(aliasName);

            //ASSERT
            sut.Should().NotBeNull();
            sut.IsHidden.Should().Be(false);
        }

        [Fact]
        public void CreateAliasWithPrivilegeUsingService()
        {
            var connection = BuildFreshDb();
            var service = BuildDataService(connection);

            var alias = BuildAlias("admin", RunAs.Admin);
            service.SaveOrUpdate(ref alias);

            var r = connection.Query<AliasQueryResult>("select * from alias");

            var results = service.Search("admin").ToArray();

            results.Should().HaveCount(1);

            results.ElementAt(0).RunAs.Should().Be(RunAs.Admin);
        }

        [Fact]
        public void FindExact()
        {
            // ARRANGE
            var sql = @"
                insert into alias(id) values (100);
                insert into alias_name(id, name, id_alias) values (1000, 'noname', 100);
                insert into alias_name(id, name, id_alias) values (2000, 'noname', 100);
                insert into alias_name(id, name, id_alias) values (3000, 'noname', 100);";

            var connection = BuildFreshDb(sql);
            var action = BuildAliasDbAction(connection);

            // ACT
            var found = action.GetExact("noname");

            // ASSERT
            using (new AssertionScope())
            {
                found.Should().NotBeNull();
                found.Name.Should().Be("noname");
            }
        }

        [Fact]
        public void FindNoExact()
        {
            // ARRANGE
            var sql = @"
                insert into alias(id) values (100);
                insert into alias_name(id, name, id_alias) values (1000, 'noname', 100);
                insert into alias_name(id, name, id_alias) values (2000, 'noname', 100);
                insert into alias_name(id, name, id_alias) values (3000, 'noname', 100);";

            var connection = BuildFreshDb(sql);
            var action = BuildAliasDbAction(connection);

            // ACT
            var found = action.GetExact("nothing");

            // ASSERT
            using (new AssertionScope())
            {
                found.Should().BeNull();
            }
        }

        [Fact]
        public void RemoveAlias()
        {
            // ARRANGE
            var connection = BuildFreshDb();
            var action = BuildAliasDbAction(connection);

            var alias = BuildAlias();

            // ACT
            action.Create(ref alias);
            action.Remove(alias);

            //ASSERT
            var sql2 = "select count(*) from alias";
            var sql3 = "select count(*) from alias_name";

            connection.ExecuteScalar<int>(sql2).Should().Be(0);
            connection.ExecuteScalar<int>(sql3).Should().Be(0);
        }

        [Fact]
        public void RemoveAliasUsingService()
        {
            // ARRANGE
            var connection = BuildFreshDb();
            var service = BuildDataService(connection);

            var alias1 = BuildAlias("one");
            var alias2 = BuildAlias("two");
            var alias3 = BuildAlias("three");

            // ACT
            service.SaveOrUpdate(ref alias1);
            service.SaveOrUpdate(ref alias2);
            service.SaveOrUpdate(ref alias3);
            service.Remove(alias1);

            //ASSERT
            var sql2 = "select count(*) from alias";
            var sql3 = "select count(*) from alias_name";

            alias1.Id.Should().BeGreaterThan(0);
            connection.ExecuteScalar<int>(sql2).Should().Be(2);
            connection.ExecuteScalar<int>(sql3).Should().Be(2);
        }

        [Fact]
        public void RemoveAliasWithDbActionSql()
        {
            // ARRANGE
            var sql = @"
                insert into alias (id, arguments, file_name) values (256, 'no args', 'no file name');
                insert into alias_name(name, id_alias) values ('noname', 256);

                insert into alias (id, arguments, file_name) values (257, 'no args', 'no file name');
                insert into alias_name(name, id_alias) values ('noname', 257);

                insert into alias (id, arguments, file_name) values (258, 'no args', 'no file name');
                insert into alias_name(name, id_alias) values ('noname', 258);";
            var connection = BuildFreshDb(sql);

            var action = BuildAliasDbAction(connection);

            // ACT
            action.Remove(new AliasQueryResult() { Id = 256 });

            using (new AssertionScope())
            {
                // ASSERT
                var sql2 = "select count(*) from alias where id = 256";
                connection.ExecuteScalar<int>(sql2).Should().Be(0);

                var sql3 = "select count(*) from alias_name where id_alias = 256";
                connection.ExecuteScalar<int>(sql3).Should().Be(0);
            }
        }

        [Fact]
        public void RemoveAliasWithServiceSql()
        {
            // ARRANGE
            var sql = @"
                insert into alias (id, arguments, file_name) values (256, 'no args', 'no file name');
                insert into alias_name(name, id_alias) values ('noname', 256);

                insert into alias (id, arguments, file_name) values (257, 'no args', 'no file name');
                insert into alias_name(name, id_alias) values ('noname', 257);

                insert into alias (id, arguments, file_name) values (258, 'no args', 'no file name');
                insert into alias_name(name, id_alias) values ('noname', 258);";
            var connection = BuildFreshDb(sql);

            var service = BuildDataService(connection);

            // ACT
            service.Remove(new AliasQueryResult() { Id = 256 });

            // ASSERT
            var sql2 = "select count(*) from alias where id = 256";
            connection.ExecuteScalar<int>(sql2).Should().Be(0);

            var sql3 = "select count(*) from alias_name where id_alias = 256";
            connection.ExecuteScalar<int>(sql3).Should().Be(0);
        }

        #endregion Methods
    }
}