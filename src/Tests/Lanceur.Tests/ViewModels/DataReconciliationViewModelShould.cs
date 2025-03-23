using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Tests.ViewModels.Generators;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class DataReconciliationViewModelShould : ViewModelTester<DataReconciliationViewModel>
{
    #region Constructors

    public DataReconciliationViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddApplicationSettings(
                             stg => visitors?.VisitSettings?.Invoke(stg)
                         )
                         .AddSingleton<IMappingService, AutoMapperMappingService>()
                         .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                         .AddMockSingleton<IDatabaseConfigurationService>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddSingleton<IReconciliationService, ReconciliationService>()
                         .AddSingleton<IPackagedAppSearchService, PackagedAppSearchService>()
                         .AddMockSingleton<IUserInteractionService>(
                             (sp, i) => visitors?.VisitUserInteractionService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserNotificationService>(
                             (sp, i) => visitors?.VisitUserNotificationService?.Invoke(sp, i) ?? i
                         );

        return serviceCollection;
    }

    [Theory]
    [ClassData(typeof(DoubloonGenerator))]
    public async Task FixDoubloons(SqlBuilder sqlBuilder)
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);

                foreach (var item in viewModel.Aliases) item.IsSelected = true;

                await viewModel.MergeCommand.ExecuteAsync(null);
                viewModel.Aliases.Should().HaveCount(0);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task NotDeleteWhenMergingDoubloons()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        var fileName = Guid.NewGuid().ToString();
        var arguments = Guid.NewGuid().ToString();
        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             fileName,
                                             arguments,
                                             props: new(DeletedAt: DateTime.Now),
                                             cfg: alias => alias.WithSynonyms("a1", "a2")
                                         )
                                         .AppendAlias(
                                             2,
                                             fileName,
                                             arguments,
                                             cfg: alias =>  alias.WithSynonyms("b1", "b2")
                                         )
                                         .AppendAlias(
                                             3,
                                             fileName,
                                             arguments,
                                             cfg: alias =>  alias.WithSynonyms("c1", "c2")
                                         );
        await TestViewModelAsync(
            async (viewModel, db) =>
            {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);

                foreach (var item in viewModel.Aliases) item.IsSelected = true;

                await viewModel.MergeCommand.ExecuteAsync(null);
                viewModel.Aliases.Should().HaveCount(0);
                
                const string sql = "select count(*) from alias where deleted_at is not null";
                db.WithConnection(c => c.ExecuteScalar(sql)).Should().Be(0, "merging should undelete alias");
            },
            sqlBuilder,
            visitors
        );
    }
    
    [Fact]
    public async Task HaveDoubloonsWithoutParameters()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             "FileName",
                                             "null",
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a1", "a2", "a3")
                                                      .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                             }
                                         )
                                         .AppendAlias(
                                             2,
                                             "FileName",
                                             "null",
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a4", "a5", "a6")
                                                      .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Should().HaveCount(2);
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [ClassData(typeof(DoubloonWithNullGenerator))]
    public async Task NotHaveDoubloonWithNullValues(string description, int doubloons, SqlBuilder sqlBuilder)
    {
        OutputHelper.WriteLine($"Test description: {description}");
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Should().HaveCount(doubloons);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task MergeDoubloonWithExpectedDetails()
    {
        var fileName = Guid.NewGuid().ToString();
        var arguments = Guid.NewGuid().ToString();
        var now = DateTime.Now;
        var timeOffset = 0;

        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.InteractAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<object>()
                 )
                 .Returns((IsConfirmed: true, DataContext: null));
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             fileName,
                                             arguments,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a1", "a2", "a3")
                                                      .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" })
                                                      .WithUsage(
                                                          now.AddHours(++timeOffset),
                                                          now.AddHours(++timeOffset),
                                                          now.AddHours(++timeOffset),
                                                          now.AddHours(++timeOffset)
                                                      );
                                             }
                                         )
                                         .AppendAlias(
                                             2,
                                             fileName,
                                             arguments,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a4", "a5", "a6")
                                                      .WithArguments(new() { ["params3"] = "params three", ["params4"] = "params four" })
                                                      .WithUsage(
                                                          now.AddHours(++timeOffset),
                                                          now.AddHours(++timeOffset),
                                                          now.AddHours(++timeOffset),
                                                          now.AddHours(++timeOffset)
                                                      );
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, db) =>
            {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);

                foreach (var item in viewModel.Aliases) item.IsSelected = true;

                await viewModel.MergeCommand.ExecuteAsync(null);

                // assert
                viewModel.Aliases.Should().HaveCount(0);
                using (new AssertionScope())
                {
                    const string sql = "select name from alias_argument where id_alias = @IdAlias";
                    var items = db.WithConnection(c => c.Query<string>(sql, new { IdAlias = 1 }));
                    items.Should().HaveCount(4);

                    const string sql2 = "select name from alias_name where id_alias = @IdAlias";
                    var items2 = db.WithConnection(c => c.Query<string>(sql2, new { IdAlias = 1 }));
                    items2.Should().HaveCount(6);

                    const string sql3 = "select id from alias_usage where id_alias = @IdAlias";
                    var items3 = db.WithConnection(c => c.Query<string>(sql3, new { IdAlias = 1 }));
                    items3.Should().HaveCount(8);
                }
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task RemoveDoubloons()
    {
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };
        var fileName = Guid.NewGuid().ToString();
        var arguments = Guid.NewGuid().ToString();

        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             fileName,
                                             arguments,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a1", "a2", "a3")
                                                      .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                             }
                                         )
                                         .AppendAlias(
                                             2,
                                             fileName,
                                             arguments,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a4", "a5", "a6")
                                                      .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                             }
                                         )
                                         .AppendAlias(
                                             3,
                                             fileName,
                                             arguments,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("a4", "a5", "a6")
                                                      .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.ShowDoubloonsCommand.ExecuteAsync(null);
                viewModel.Aliases.Should().HaveCount(3);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task ShowAliasesWithoutNotes()
    {
        await TestViewModelAsync(
            async (viewModel, _) => await viewModel.ShowAliasesWithoutNotesCommand.ExecuteAsync(null),
            SqlBuilder.Empty
        );
    }

    [Fact]
    public async Task ShowBrokenAliasesAsync()
    {
        await TestViewModelAsync(
            async (viewModel, _) => await viewModel.ShowBrokenAliasesCommand.ExecuteAsync(null),
            SqlBuilder.Empty
        );
    }

    [Fact]
    public async Task ShowDoubloons()
    {
        await TestViewModelAsync(
            async (viewModel, _) => await viewModel.ShowDoubloonsCommand.ExecuteAsync(null),
            SqlBuilder.Empty
        );
    }

    [Fact]
    public async Task ShowRestoreAlias()
    {
        await TestViewModelAsync(
            async (viewModel, _) =>  await viewModel.ShowRestoreAliasCommand.ExecuteAsync(null),
            SqlBuilder.Empty
        );
    }

    #endregion
}