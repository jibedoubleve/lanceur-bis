using AutoMapper;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.SQLite;
using Lanceur.Tests.Logging;
using Lanceur.Tests.SQL;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Utils.Builders;
using Lanceur.Utils;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class KeywordViewModelShould : SQLiteTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion Fields

    #region Constructors

    public KeywordViewModelShould(ITestOutputHelper output)
    {
        _output = output;
    }

    #endregion Constructors

    #region Methods

    [Fact]
    public void AbleToAddMultipleTimesAlias()
    {
        new TestScheduler().With(scheduler =>
        {
            // ARRANGE
            var dbRepository = Substitute.For<IDbRepository>();
            dbRepository.CheckNamesExist(Arg.Any<string[]>())
                        .Returns(new ExistingNameResponse(Array.Empty<string>()));

            var packageValidator = Substitute.For<IPackagedAppValidator>();
            
            var vm = new KeywordsViewModelBuilder()
                     .With(scheduler)
                     .With(_output)
                     .With(dbRepository)
                     .With(packageValidator)
                     .Build();

            var synonyms = Guid.NewGuid().ToString(); 
            var fileName = Guid.NewGuid().ToString();
            
            // ACT
            
            vm.Activate(new());
            scheduler.Start();
            vm.CreatingAlias.Execute().Subscribe();
            var hash = vm.SelectedAlias.GetHashCode();
            
            vm.SelectedAlias.Synonyms = synonyms;
            vm.SelectedAlias.FileName = fileName;
            
            packageValidator.FixAsync(Arg.Any<AliasQueryResult>())
                            .Returns(vm.SelectedAlias);

            vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();
            
            // ASSERT
            vm.SelectedAlias.GetHashCode().Should().Be(hash);

        });
    }
    
    [Fact]
    public void AbleToRemoveSynonym()
    {
        new TestScheduler().With(scheduler =>
        {
            // ARRANGE
            const long idAlias = 10;
            var sql = new SqlBuilder().AppendAlias(idAlias, "@multi@", "@alias2@@alias3", "multi1", "multi2", "multi3")
                                      .AppendAlias("alias2", "action1", "alias2")
                                      .AppendAlias("alias3", "action2", "alias3")
                                      .ToString();
            
            var connectionMgr = new SQLiteSingleConnectionManager(BuildFreshDb(sql));
            
            var logger = new XUnitLoggerFactory(_output);
            var cfg = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();
            });
            var conversionService = new AutoMapperConverter(new Mapper(cfg));
            var dbRepository = new SQLiteRepository(connectionMgr, logger, conversionService);
            
            var packageValidator = Substitute.For<IPackagedAppValidator>();
            var vm = new KeywordsViewModelBuilder()
                     .With(scheduler)
                     .With(logger)
                     .With(dbRepository)
                     .With(packageValidator)
                     .Build();

            // ACT
            vm.Activate(new());
            vm.SearchQuery = "multi";
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(20).Ticks);
            
            // It's only now I know what is returned by 'FixAsync'
            packageValidator.FixAsync(Arg.Any<AliasQueryResult>())
                            .Returns(vm.SelectedAlias);
            
            vm.SelectedAlias.Synonyms = "multi1, multi2";
            vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();
            scheduler.Start();

            // ASSERT
            using (new AssertionScope())
            {
                // The database should have one less synonym
                var countSql = $"select count(*) from alias_name where id_alias = {idAlias}";
                connectionMgr.WithinTransaction(tx => (long)tx.Connection.ExecuteScalar(countSql))
                             .Should().Be(2);
                // And the UI also...
                vm.Aliases.Should().HaveCount(2);
            }
        });
    }

    #endregion Methods
}