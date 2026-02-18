using System.Data;
using Bogus;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Tests.Repositories;

public class SQLiteAliasRepositoryShould : TestBase
{
    #region Constructors

    public SQLiteAliasRepositoryShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

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

    private AliasDbAction BuildAliasDbAction(ILoggerFactory loggerFactory = null)
    {
        var log = loggerFactory ?? CreateLoggerFactory();
        var action = new AliasDbAction(log);
        return action;
    }

    private AliasSearchDbAction BuildAliasSearchDbAction()
    {
        var log = CreateLoggerFactory();
        return new(log, new DbActionFactory(log));
    }

    private SQLiteAliasRepository BuildDataService(IDbConnection connection)
    {
        var scope = new DbSingleConnectionManager(connection);
        var log = CreateLoggerFactory();
        var service = new SQLiteAliasRepository(
            scope,
            log,
            new DbActionFactory(log)
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

        var connection = BuildFreshDb(sql, ConnectionStringFactory.InMemory);
        var c = new DbSingleConnectionManager(connection);
        var aliasAction = BuildAliasDbAction();
        var aliasSearch = BuildAliasSearchDbAction();

        // ACT & ASSERT
        // -- Find the alias
        var alias = c.WithinTransaction(tx => aliasSearch.Search(tx, "noname_1").SingleOrDefault());
        alias.ShouldSatisfyAllConditions(
            a => a.Id.ShouldBe(1001, "this is the id of the alias to find"),
            a => a.ShouldNotBeNull("the search matches one alias")
        );

        // -- Add new names to the alias and save it
        alias.Synonyms += ", noname_4, noname_5";
        var id = alias.Id;
        var outputId = c.WithinTransaction(tx => aliasAction.SaveOrUpdate(tx, ref alias));
        outputId.ShouldBe(id, "the alias has only be updated");

        // -- Retrieve back the alias and check the names
        var found = c.WithinTransaction(tx => aliasSearch.Search(tx, "noname_1").SingleOrDefault());
        found.ShouldSatisfyAllConditions(
            f => f.ShouldNotBeNull(),
            f => f.Synonyms.SplitCsv().Length.ShouldBe(5)
        );
    }

    [Fact]
    public void BeAbleToFindById()
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
        var found = c.WithinTransaction(tx => action.GetById(tx, 100));

        // ASSERT
        found.ShouldSatisfyAllConditions(
            f => f.ShouldNotBeNull(),
            f => f.Name.ShouldBe("noname")
        );
    }

    [Fact]
    public void BeAbleToNotFindById()
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
        var found = c.WithinTransaction(tx => action.GetById(tx, 1000));

        // ASSERT
        found.ShouldBeNull();
    }

    [Fact]
    public void CreateAlias()
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

        var connection = BuildFreshDb(sql, ConnectionStringFactory.InMemory);
        var action = BuildAliasDbAction();

        var alias1 = BuildAlias();
        var alias2 = BuildAlias();
        var alias3 = BuildAlias();
        var c = new DbSingleConnectionManager(connection);

        // ACT
        c.WithinTransaction(tx =>
            {
                action.SaveOrUpdate(tx, ref alias1);
                action.SaveOrUpdate(tx, ref alias2);
                action.SaveOrUpdate(tx, ref alias3);
            }
        );

        //ASSERT
        const string sql2 = "select count(*) from alias";
        const string sql3 = "select count(*) from alias_name";

        Assert.Multiple(
            () => alias1.Id.ShouldBe(1004),
            () => alias2.Id.ShouldBe(1005),
            () => alias3.Id.ShouldBe(1006),
            () => connection.ExecuteScalar<int>(sql2).ShouldBe(6),
            () => connection.ExecuteScalar<int>(sql3).ShouldBe(6)
        );
    }

    [Fact]
    public void CreateAliasInvisibleUsingService()
    {
        // ARRANGE
        var aliasName = Guid.NewGuid().ToString();
        var connection = BuildFreshDb(connectionString: ConnectionStringFactory.InMemory);
        var action = BuildAliasDbAction();
        var db = new DbSingleConnectionManager(connection);
        QueryResult alias1 = new AliasQueryResult { Name = aliasName };
        db.WithinTransaction(tx => action.CreateInvisible(tx, ref alias1));

        // ACT
        var sut = db.WithinTransaction(tx => action.GetById(tx, alias1.Id));

        //ASSERT
        sut.ShouldNotBeNull();
        sut.IsHidden.ShouldBe(true);
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
        const string sql2 = "select count(*) from alias;";
        const string sql3 = "select count(*) from alias_name;";
        Assert.Multiple(
            () => alias1.Name.ShouldBe(name1),
            () => alias2.Name.ShouldBe(name2),
            () => alias3.Name.ShouldBe(name3),
            () => connection.ExecuteScalar<int>(sql2).ShouldBe(6),
            () => connection.ExecuteScalar<int>(sql3).ShouldBe(6)
        );
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

        var sut = service.GetById(alias1.Id);

        //ASSERT
        sut.ShouldNotBeNull();
        sut.IsHidden.ShouldBe(false);
    }

    [Fact]
    public void CreateAliasWithAdditionalParameters()
    {
        // ARRANGE
        var sb = new ServiceCollection().AddLogging(b => b.AddXUnit(OutputHelper))
                                        .BuildServiceProvider();

        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms("Alias")).ToSql();

        var connection = BuildFreshDb(sql, ConnectionStringFactory.InMemory);
        var dbAction = BuildAliasDbAction(sb.GetService<ILoggerFactory>());

        var alias = new AliasQueryResult { Id = 1, Name = "Alias" };

        // ACT
        alias.AdditionalParameters.Add(new() { AliasId = 1, Name = "someName", Parameter = "someParameter" });
        dbAction.SaveOrUpdate(connection.BeginTransaction(), ref alias);

        // ASSERT
        const string sqlCount = "select count(*) from alias_argument";
        connection.ExecuteScalar<int>(sqlCount).ShouldBe(1);
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
        sut.ShouldSatisfyAllConditions(
            s => s.ShouldNotBeNull(),
            s => s.Id.ShouldNotBe(0),
            s => s.Name.ShouldBe(name),
            s => s.RunAs.ShouldBe(RunAs.Admin)
        );
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
        c.WithinTransaction(tx =>
            {
                action.SaveOrUpdate(tx, ref alias);
                action.Remove(tx, alias);
            }
        );

        //ASSERT
        const string sql2 = "select count(*) from alias";
        const string sql3 = "select count(*) from alias_name";

        connection.ExecuteScalar<int>(sql2).ShouldBe(0);
        connection.ExecuteScalar<int>(sql3).ShouldBe(0);
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
        service.RemoveLogically(alias1);

        //ASSERT
        const string sql2 = "select count(*) from alias where deleted_at is null";
        const string sql3 = "select count(*) from alias_name";

        alias1.Id.ShouldBeGreaterThan(0);
        connection.ExecuteScalar<int>(sql2).ShouldBe(2);
        connection.ExecuteScalar<int>(sql3).ShouldBe(3); // Logical deletion don't remove data: the names of the
        // deleted alias should remain in the database
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

        // ASSERT
        const string sql2 = "select count(*) from alias where id = 256";
        const string sql3 = "select count(*) from alias_name where id_alias = 256";
        connection.ShouldSatisfyAllConditions(
            co => co.ExecuteScalar<int>(sql2).ShouldBe(0),
            co => co.ExecuteScalar<int>(sql3).ShouldBe(0)
        );
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
        service.RemoveLogically((AliasQueryResult)new() { Id = 256 });

        // ASSERT
        const string sql2 = "select count(*) from alias where id = 256 and deleted_at is null";
        connection.ExecuteScalar<int>(sql2).ShouldBe(0);

        const string sql3 = "select count(*) from alias_name where id_alias = 256";
        // The logical deletion don't remove data, the names should therefore
        // remain in the database
        connection.ExecuteScalar<int>(sql3).ShouldBe(1);
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
        sut.ShouldSatisfyAllConditions(
            s => s.ShouldNotBeEmpty(),
            s => s.Length.ShouldBe(1),
            s => s.ElementAt(0).RunAs.ShouldBe(RunAs.Admin)
        );
    }
    #endregion
}