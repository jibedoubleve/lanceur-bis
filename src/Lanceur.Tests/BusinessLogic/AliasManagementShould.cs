using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
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
                Parameters = string.Empty,
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
            var log = Substitute.For<IAppLoggerFactory>();
            var action = new AliasDbAction(scope, log);
            return action;
        }

        private static SQLiteRepository BuildDataService(SQLiteConnection connection)
        {
            var scope = new SQLiteConnectionScope(connection);
            var log = Substitute.For<IAppLoggerFactory>();
            var conv = Substitute.For<IConvertionService>();
            var service = new SQLiteRepository(scope, log, conv);
            return service;
        }

        [Fact]
        public void UpdateAlias()
        {
            // ARRANGE
            // I'm aware about SQL injection... But I guess risk is low.
            var oldName = $"OLD_{Guid.NewGuid()}";
            var oldParameters = $"OLD_{Guid.NewGuid()}";
            var sql = @$"
                insert into alias(id, arguments) values (1000, '{oldParameters}');
                insert into alias_name(id, id_alias, name) values (1000, 1000, '{oldName}');";

            var connection = BuildFreshDB(sql);
            var action = BuildAliasDbAction(connection);

            var newName = $"NEW_{Guid.NewGuid()}";
            var newParameters = $"NEW_{Guid.NewGuid()}";
            var alias = new AliasQueryResult
            {
                Id = 1000,
                Name = newName,
                Parameters = newParameters
            };

            // ACT
            var act = () => action.Update(alias);

            // ASSERT
            using (new AssertionScope())
            {
                act.Should().NotThrow();
                
                var sql2 = @"
                    select 
	                    a.id        as id,
	                    an.name     as name,
                        a.arguments as parameters
                    from 
	                    alias a 
                        inner join alias_name an on a.id = an.id_alias
                    where 
                        a.id = 1000";

                var assertedAlias = connection.Query<AliasQueryResult>(sql2).FirstOrDefault();

                assertedAlias.Name.Should().Be(oldName); // To update name use UpdateName
                assertedAlias.Parameters.Should().Be(newParameters);
            }
        }

        [Fact]
        public void CreateAlias()
            {
                // ARRANGE
                var sql = @"
                insert into alias_name(id, name, id_alias) values (1000, 'noname', null);
                insert into alias_name(id, name, id_alias) values (2000, 'noname', null);
                insert into alias_name(id, name, id_alias) values (3000, 'noname', null);";

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
                using (new AssertionScope())
                {
                    alias1.Id.Should().Be(7 + 1);
                    alias2.Id.Should().Be(7 + 2);
                    alias3.Id.Should().Be(7 + 3);

                    var sql2 = "select count(*) from alias";
                    var sql3 = "select count(*) from alias_name";

                    connection.ExecuteScalar<int>(sql2).Should().Be(7 + 3);
                    connection.ExecuteScalar<int>(sql3).Should().Be(7 + 6);
                }
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
                insert into alias_name(id, name, id_alias) values (1000, 'noname', null);
                insert into alias_name(id, name, id_alias) values (2000, 'noname', null);
                insert into alias_name(id, name, id_alias) values (3000, 'noname', null);";

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

                alias1.Id.Should().Be(7 + 1);
                alias2.Id.Should().Be(7 + 2);
                alias3.Id.Should().Be(7 + 3);

                connection.ExecuteScalar<int>(sql2).Should().Be(7 + 3);
                connection.ExecuteScalar<int>(sql3).Should().Be(7 + 6);
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

                connection.ExecuteScalar<int>(sql2).Should().Be(7); // Default alias are in the DB
                connection.ExecuteScalar<int>(sql3).Should().Be(7);
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
                connection.ExecuteScalar<int>(sql2).Should().Be(9);
                connection.ExecuteScalar<int>(sql3).Should().Be(9);
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