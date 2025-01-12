
using System.Data;
using Bogus;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Tests.Tooling;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Tests.BusinessLogic;

public class AliasManagementShould : TestBase
{
    #region Constructors

    public AliasManagementShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private static AliasQueryResult BuildAlias(string name = null, RunAs runAs = RunAs.CurrentUser)
    {
        var faker = new Faker();
        name ??= faker.Lorem.Word();
        return new()
        {
            Name = name, 
            Synonyms = name,
            Parameters = string.Empty,
            FileName = faker.System.FileName(),
            RunAs = runAs,
            StartMode = StartMode.Default,
            WorkingDirectory = faker.System.DirectoryPath(),
            Icon = faker.Image.PlaceholderUrl(150, 150)
        };
    }

    private static AliasDbAction BuildAliasDbAction()
    {
        var log = Substitute.For<ILoggerFactory>();
        var action = new AliasDbAction(log);
        return action;
    }

    private AliasSearchDbAction BuildAliasSearchDbAction()
    {
        var log = Substitute.For<ILoggerFactory>();
        return new(log, new DbActionFactory(new AutoMapperMappingService(), log));
    }

    private static SQLiteAliasRepository BuildDataService(IDbConnection connection)
    {
        var scope = new DbSingleConnectionManager(connection);
        var log = Substitute.For<ILoggerFactory>();
        var conv = Substitute.For<IMappingService>();
        var service = new SQLiteAliasRepository(
            scope,
            log,
            conv,
            new DbActionFactory(new AutoMapperMappingService(), log)
        );
        return service;
    }

    [Fact]
    public void AddNewNames()
    {
        // ARRANGE
        const string sql = """
                           insert into alias(id) values (1001);
                           insert into alias(id) values (1002);
                           insert into alias(id) values (1003);

                           insert into alias_name(id, name, id_alias) values (100, 'noname_1', 1001);
                           insert into alias_name(id, name, id_alias) values (101, 'noname_2', 1001);
                           insert into alias_name(id, name, id_alias) values (102, 'noname_3', 1001);
                           insert into alias_name(id, name, id_alias) values (200, 'some_name_1', 1002);
                           insert into alias_name(id, name, id_alias) values (300, 'some_name_2', 1003);
                           """;
        
        var connectionString = ConnectionStringFactory.InMemory;
        var connection = BuildFreshDb(sql, connectionString.ToString());
        var c = new DbSingleConnectionManager(connection);
        var aliasAction = BuildAliasDbAction();
        var aliasSearch = BuildAliasSearchDbAction();

        AliasQueryResult alias;
        // ACT
        
        // Find the alias
        alias = c.WithinTransaction(tx => aliasSearch.Search(tx, "noname_1").SingleOrDefault());
        using (new AssertionScope())
        {
            alias.Id.Should().Be(1001, "this is the id of the alias to find");
            alias.Should().NotBeNull("the search matches one alias");
        }
        
        // Add new names to the alias and save it
        alias.Synonyms += ", noname_4, noname_5";
        var id = alias.Id;
        var outputId = c.WithinTransaction(tx => aliasAction.SaveOrUpdate(tx, ref alias));
        outputId.Should().Be(id, "the alias has only be updated");
        
        // Retrieve back the alias and check the names
        var found = c.WithinTransaction(tx => aliasSearch.Search(tx, "noname_1").SingleOrDefault());
        using (new AssertionScope())
        {
            found.Should().NotBeNull();
            found.Synonyms.SplitCsv().Should().HaveCount(5);
        }
    }

    [Fact]
    public void CreateAlias()
    {
        // ARRANGE
        const string sql = $"""
                           insert into alias(id) values (1001);
                           insert into alias(id) values (1002);
                           insert into alias(id) values (1003);

                           insert into alias_name(id, name, id_alias) values (1000, 'noname', 1001);
                           insert into alias_name(id, name, id_alias) values (2000, 'noname', 1002);
                           insert into alias_name(id, name, id_alias) values (3000, 'noname', 1003);
                           """;

        var connectionString = ConnectionStringFactory.InMemory;
        var connection = BuildFreshDb(sql, connectionString.ToString());
        var action = BuildAliasDbAction();

        var alias1 = BuildAlias();
        var alias2 = BuildAlias();
        var alias3 = BuildAlias();
        var c = new DbSingleConnectionManager(connection);

        // ACT
        c.WithinTransaction(
            tx =>
            {
                action.SaveOrUpdate(tx, ref alias1);
                action.SaveOrUpdate(tx, ref alias2);
                action.SaveOrUpdate(tx, ref alias3);
            }
        );

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
        var action = BuildAliasDbAction();
        var c = new DbSingleConnectionManager(connection);
        QueryResult alias1 = new AliasQueryResult { Name = aliasName };
        c.WithinTransaction(tx => action.CreateInvisible(tx, ref alias1));

        // ACT
        var sut = c.WithinTransaction(tx => action.GetExact(tx, aliasName, true));

        //ASSERT
        sut.Should().NotBeNull();
        sut.IsHidden.Should().Be(true);
    }

    [Fact]
    public void CreateAliasUsingService()
    {
        // ARRANGE
        const string sql = """
                           insert into alias(id) values (1001);
                           insert into alias(id) values (1002);
                           insert into alias(id) values (1003);

                           insert into alias_name(id, name, id_alias) values (1000, 'noname', 1001);
                           insert into alias_name(id, name, id_alias) values (2000, 'noname', 1002);
                           insert into alias_name(id, name, id_alias) values (3000, 'noname', 1003);
                           """;

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
            const string sql2 = "select count(*) from alias;";
            const string sql3 = "select count(*) from alias_name;";

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
        // ARRANGE
        var connection = BuildFreshDb();
        var service = BuildDataService(connection);

        const string name = "admin";
        var alias = BuildAlias(name, RunAs.Admin);
        service.SaveOrUpdate(ref alias);

        const string sql = """
                           select 
                               a.id, 
                               an.name, 
                               a.run_as as RunAs
                           from 
                           	alias a
                           	inner join alias_name an on an.id_alias = a.id
                           where a.id = @id
                           """;
        var sut = connection.Query<AliasQueryResult>(sql, new { id = alias.Id })
                            .Single();
        // ASSERT
        using (new AssertionScope())
        {
            sut.Should().NotBeNull();
            sut.Id.Should().NotBe(0);
            sut.Name.Should().Be(name);
            sut.RunAs.Should().Be(RunAs.Admin);
        }
    }

    [Fact]
    public void FindExact()
    {
        // ARRANGE
        const string sql = """
                           insert into alias(id) values (100);
                           insert into alias_name(id, name, id_alias) values (1000, 'noname', 100);
                           insert into alias_name(id, name, id_alias) values (2000, 'noname', 100);
                           insert into alias_name(id, name, id_alias) values (3000, 'noname', 100);
                           """;

        var connection = BuildFreshDb(sql);
        var action = BuildAliasDbAction();
        var c = new DbSingleConnectionManager(connection);

        // ACT
        var found = c.WithinTransaction(tx => action.GetExact(tx, "noname"));

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
        const string sql = """
                           insert into alias(id) values (100);
                           insert into alias_name(id, name, id_alias) values (1000, 'noname', 100);
                           insert into alias_name(id, name, id_alias) values (2000, 'noname', 100);
                           insert into alias_name(id, name, id_alias) values (3000, 'noname', 100);
                           """;

        var connection = BuildFreshDb(sql);
        var action = BuildAliasDbAction();
        var c = new DbSingleConnectionManager(connection);

        // ACT
        var found = c.WithinTransaction(tx => action.GetExact(tx, "nothing"));

        // ASSERT
        using (new AssertionScope()) { found.Should().BeNull(); }
    }

    [Fact]
    public void RemoveAlias()
    {
        // ARRANGE
        var connection = BuildFreshDb();
        var action = BuildAliasDbAction();
        var c = new DbSingleConnectionManager(connection);

        var alias = BuildAlias();

        // ACT
        c.WithinTransaction(
            tx =>
            {
                action.SaveOrUpdate(tx, ref alias);
                action.Remove(tx, alias);
            }
        );

        //ASSERT
        const string sql2 = "select count(*) from alias";
        const string sql3 = "select count(*) from alias_name";

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
        const string sql2 = "select count(*) from alias where deleted_at is null";
        const string sql3 = "select count(*) from alias_name";

        alias1.Id.Should().BeGreaterThan(0);
        connection.ExecuteScalar<int>(sql2).Should().Be(2);
        connection.ExecuteScalar<int>(sql3).Should().Be(3); // Logical deletion don't remove data: the names of the
        // deleted alias should remains in the database
    }

    [Fact]
    public void RemoveAliasWithDbActionSql()
    {
        // ARRANGE
        const string sql = """
                           insert into alias (id, arguments, file_name) values (256, 'no args', 'no file name');
                           insert into alias_name(name, id_alias) values ('noname', 256);

                           insert into alias (id, arguments, file_name) values (257, 'no args', 'no file name');
                           insert into alias_name(name, id_alias) values ('noname', 257);

                           insert into alias (id, arguments, file_name) values (258, 'no args', 'no file name');
                           insert into alias_name(name, id_alias) values ('noname', 258);
                           """;
        var connection = BuildFreshDb(sql);
        var c = new DbSingleConnectionManager(connection);

        var action = BuildAliasDbAction();

        // ACT
        c.WithinTransaction(tx => action.Remove(tx, new()  { Id = 256 }));

        using (new AssertionScope())
        {
            // ASSERT
            const string sql2 = "select count(*) from alias where id = 256";
            connection.ExecuteScalar<int>(sql2).Should().Be(0);

            const string sql3 = "select count(*) from alias_name where id_alias = 256";
            connection.ExecuteScalar<int>(sql3).Should().Be(0);
        }
    }

    [Fact]
    public void RemoveAliasWithServiceSql()
    {
        // ARRANGE
        const string sql = """
                           insert into alias (id, arguments, file_name) values (256, 'no args', 'no file name');
                           insert into alias_name(name, id_alias) values ('noname', 256);

                           insert into alias (id, arguments, file_name) values (257, 'no args', 'no file name');
                           insert into alias_name(name, id_alias) values ('noname', 257);

                           insert into alias (id, arguments, file_name) values (258, 'no args', 'no file name');
                           insert into alias_name(name, id_alias) values ('noname', 258);
                           """;
        var connection = BuildFreshDb(sql);

        var service = BuildDataService(connection);

        // ACT
        service.Remove((AliasQueryResult)new() { Id = 256 });

        // ASSERT
        const string sql2 = "select count(*) from alias where id = 256 and deleted_at is null";
        connection.ExecuteScalar<int>(sql2).Should().Be(0);

        const string sql3 = "select count(*) from alias_name where id_alias = 256";
        // The logical deletion don't remove data, the names should therefore
        // remain in the database
        connection.ExecuteScalar<int>(sql3).Should().Be(1);
    }

    [Fact]
    public void RetrieveAliasWithPrivilegeUsingService()
    {
        // ARRANGE
        var connection = BuildFreshDb();
        var service = BuildDataService(connection);

        const string sql = """
                           insert into alias (id, run_as) values (1, 0);
                           insert into alias_name(id, name, id_alias) values (1, 'admin', 1);
                           """;
        connection.Execute(sql);

        // ACT
        var sut = service.Search("admin").ToArray();

        // ASSERT
        using (new AssertionScope())
        {
            sut.Should().NotBeEmpty();
            sut.Should().HaveCount(1);
            sut.ElementAt(0).RunAs.Should().Be(RunAs.Admin);
        }
    }

    #endregion
}