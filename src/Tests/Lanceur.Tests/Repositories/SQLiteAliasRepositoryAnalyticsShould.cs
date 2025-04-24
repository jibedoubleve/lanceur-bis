using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Repositories;

/// <summary>
///     These tests are designed to detect SQL errors.
///     Consider them as a health check for SQL queries in AliasRepository.
/// </summary>
public class SQLiteAliasRepositoryQueryShouldBeValid : TestBase

{
    #region Constructors

    public SQLiteAliasRepositoryQueryShouldBeValid(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private SQLiteAliasRepository BuildRepository(IConnectionString connectionString = null)
    {
        var connection = BuildFreshDb(connectionString: connectionString?.ToString());
        var scope = new DbSingleConnectionManager(connection);
        var log = Substitute.For<ILoggerFactory>();
        var conv = Substitute.For<IMappingService>();
        var service = new SQLiteAliasRepository(
            scope,
            log,
            conv,
            new DbActionFactory(new MappingService(), log)
        );
        return service;
    }

    [Fact] public void GetAdditionalParameter() => BuildRepository().GetAdditionalParameter([1, 2, 3]);
    
    [Fact] public void GetAliasesWithoutNotes() => BuildRepository().GetAliasesWithoutNotes();
    
    [Fact] public void GetAll() => BuildRepository().GetAll();
    
    [Fact] public void GetAllAliasWithAdditionalParameters() => BuildRepository().GetAllAliasWithAdditionalParameters();
    
    [Fact] public void GetBrokenAliases() => BuildRepository().GetBrokenAliases();
    
    [Fact] public void GetById() => BuildRepository().GetById(1);
    
    [Fact] public void GetDeletedAlias() => BuildRepository().GetDeletedAlias();
    
    [Fact] public void GetDoubloons() => BuildRepository().GetDoubloons();
    
    [Fact] public void GetExistingAliases() => BuildRepository().GetExistingAliases([], 0);
    
    [Fact] public void GetExistingDeletedAliases() => BuildRepository().GetExistingDeletedAliases([], 0);
    
    [Fact] public void GetHiddenCounters() => BuildRepository().GetHiddenCounters();
    
    [Fact] public void GetUsage() => BuildRepository().GetUsage(Per.DayOfWeek);
    
    [Fact] public void GetYearsWithUsage() => BuildRepository().GetYearsWithUsage();

    [Fact] public void HydrateAlias() => BuildRepository().HydrateAlias(new());

    [Fact] public void HydrateMacro() => BuildRepository().HydrateMacro(new AliasQueryResult());

    #endregion
}