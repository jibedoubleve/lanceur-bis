using Dapper;
using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Tests.SQLite;
using NSubstitute;
using System.Data.SQLite;
using Xunit;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Tests.BusinessLogic
{
    public class AliasManagementShould : SQLiteTest
    {
        #region Methods

        private static AliasQueryResult BuildAlias(string name = null, RunAs runAs = RunAs.CurrentUser)
        {
            return new AliasQueryResult()
            {
                Name = name ?? Guid.NewGuid().ToString(),
                Arguments = string.Empty,
                FileName = @"C:\Users\jibedoubleve\AppData\Local\Programs\beekeeper-studio\Beekeeper Studioeuh.exe",
                RunAs = runAs,
                StartMode = StartMode.Default,
                WorkingDirectory = string.Empty,
                Icon = null,
            };
        }

        private static AliasDbAction BuildAliasDbAction(SQLiteConnection connection)
        {
            var scope = new SQLiteConnectionScope(connection);
            var log = Substitute.For<ILogService>();
            var action = new AliasDbAction(scope, log);
            return action;
        }

        private static SQLiteDataService BuildDataService(SQLiteConnection connection)
        {
            var scope = new SQLiteConnectionScope(connection);
            var log = Substitute.For<ILogService>();
            var conv = Substitute.For<IConvertionService>();
            var service = new SQLiteDataService(scope, log, conv);
            return service;
        }

        [Fact]
        public void CreateAlias()
        {
            // ARRANGE
            var sql = @"
                insert into alias_name(id, name, id_alias) values (1, 'noname', null);
                insert into alias_name(id, name, id_alias) values (2, 'noname', null);
                insert into alias_name(id, name, id_alias) values (3, 'noname', null);";

            var connection = BuildFreshDB(sql);
            var action = BuildAliasDbAction(connection);

            var alias1 = BuildAlias();
            var alias2 = BuildAlias();
            var alias3 = BuildAlias();

            // ACT
            action.Create(ref alias1, 1);
            action.Create(ref alias2, 1);
            action.Create(ref alias3, 1);

            //ASSERT
            var sql2 = "select count(*) from alias";
            var sql3 = "select count(*) from alias_name";

            alias1.Id.Should().Be(1);
            alias2.Id.Should().Be(2);
            alias3.Id.Should().Be(3);


            connection.ExecuteScalar<int>(sql2).Should().Be(3);
            connection.ExecuteScalar<int>(sql3).Should().Be(6);
        }

        [Fact]
        public void CreateAliasWithPrivilegeUsingService()
        {
            var connection = BuildFreshDB();
            var service = BuildDataService(connection);

            var alias = BuildAlias("admin", RunAs.Admin);
            service.SaveOrUpdate(ref alias);

            var results = service.Search("admin");

            results.Should().HaveCount(1);

            results.ElementAt(0).RunAs.Should().Be(RunAs.Admin);

        }
        [Fact]
        public void CreateAliasUsingService()
        {

            // ARRANGE
            var sql = @"
                insert into alias_name(id, name, id_alias) values (1, 'noname', null);
                insert into alias_name(id, name, id_alias) values (2, 'noname', null);
                insert into alias_name(id, name, id_alias) values (3, 'noname', null);";

            var connection = BuildFreshDB(sql);
            var service = BuildDataService(connection);

            var alias1 = BuildAlias();
            var alias2 = BuildAlias();
            var alias3 = BuildAlias();

            // ACT
            service.SaveOrUpdate(ref alias1);
            service.SaveOrUpdate(ref alias2);
            service.SaveOrUpdate(ref alias3);

            //ASSERT
            var sql2 = "select count(*) from alias;";
            var sql3 = "select count(*) from alias_name;";

            alias1.Id.Should().Be(1);
            alias2.Id.Should().Be(2);
            alias3.Id.Should().Be(3);

            connection.ExecuteScalar<int>(sql2).Should().Be(3);
            connection.ExecuteScalar<int>(sql3).Should().Be(6);
        }

        [Fact]
        public void RemoveAlias()
        {
            // ARRANGE
            var connection = BuildFreshDB();
            var action = BuildAliasDbAction(connection);

            var alias = BuildAlias();

            // ACT
            action.Create(ref alias, 1);
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
            var connection = BuildFreshDB();
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
        public void RemoveAliasWithDbActionSQL()
        {
            // ARRANGE
            var sql = @"
                insert into alias (id, arguments, file_name, id_session) values (256, 'no args', 'no file name', 1);
                insert into alias_name(name, id_alias) values ('noname', 256);

                insert into alias (id, arguments, file_name, id_session) values (257, 'no args', 'no file name', 1);
                insert into alias_name(name, id_alias) values ('noname', 257);

                insert into alias (id, arguments, file_name, id_session) values (258, 'no args', 'no file name', 1);
                insert into alias_name(name, id_alias) values ('noname', 258);";
            var connection = BuildFreshDB(sql);

            var action = BuildAliasDbAction(connection);

            // ACT
            action.Remove(new AliasQueryResult() { Id = 256 });

            // ASSERT
            var sql2 = "select count(*) from alias where id = 256";
            connection.ExecuteScalar<int>(sql2).Should().Be(0);

            var sql3 = "select count(*) from alias_name where id_alias = 256";
            connection.ExecuteScalar<int>(sql3).Should().Be(0);
        }

        [Fact]
        public void RemoveAliasWithServiceSQL()
        {
            // ARRANGE
            var sql = @"
                insert into alias (id, arguments, file_name, id_session) values (256, 'no args', 'no file name', 1);
                insert into alias_name(name, id_alias) values ('noname', 256);

                insert into alias (id, arguments, file_name, id_session) values (257, 'no args', 'no file name', 1);
                insert into alias_name(name, id_alias) values ('noname', 257);

                insert into alias (id, arguments, file_name, id_session) values (258, 'no args', 'no file name', 1);
                insert into alias_name(name, id_alias) values ('noname', 258);";
            var connection = BuildFreshDB(sql);

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