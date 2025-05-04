using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Mappers;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Generators;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
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
                         .AddMockSingleton<IViewFactory>()
                         .AddSingleton<IMappingService, MappingService>()
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

    [Fact]
    public async Task DeletePermanentlyWithValidation()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) =>
            {
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(true);
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
            1,
            "name",
            props: new() { DeletedAt = DateTime.Now },
            cfg: a =>
            {
                a.WithSynonyms("a1", "a2");
                a.WithArguments(new() { ["1"] = "un", ["2"] = "deux", ["3"] = "trois" });
                a.WithUsage(
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025")
                );
            }
        );
        await TestViewModelAsync(
            async (viewModel, db) =>
            {
                // arrange
                await viewModel.ShowRestoreAliasesCommand.ExecuteAsync(null);

                // act
                viewModel.Aliases.ElementAt(0).IsSelected = true;
                await viewModel.DeletePermanentlyCommand.ExecuteAsync(null);

                // assert
                db.WithConnection(
                    c =>
                    {
                        using (new AssertionScope())
                        {
                            c.ExecuteScalar("select count(*) from alias_usage where id_alias = 1")
                             .Should().Be(0, "usage should be cleared");

                            c.ExecuteScalar("select count(*) from alias_name where id_alias = 1")
                             .Should().Be(0, "names should be cleared");

                            c.ExecuteScalar("select count(*) from alias_argument where id_alias = 1")
                             .Should().Be(0, "arguments should be cleared");

                            c.ExecuteScalar("select count(*) from alias where id = 1")
                             .Should().Be(0, "alias should be cleared");
                        }
                    }
                );
            },
            sqlBuilder,
            visitors
        );
    }

    [Theory]
    [InlineData("A", 1)]
    [InlineData("B", 1)]
    [InlineData("C", 1)]
    [InlineData("E", 0)]
    public async Task FilterInactiveAliases(string filter, int count)
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.AskUserYesNoAsync(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.InactivityThreshold = 2
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("A");
                                                 alias.WithUsage(DateTime.Now.AddMonths(-10));
                                             }
                                         )
                                         .AppendAlias(
                                             2,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("B");
                                                 alias.WithUsage(DateTime.Now.AddMonths(-10));
                                             }
                                         )
                                         .AppendAlias(
                                             3,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("C");
                                                 alias.WithUsage(DateTime.Now.AddMonths(-10));
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.SetInactivityThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowInactiveAliasesCommand.ExecuteAsync(null);
                await viewModel.FilterAliasCommand.ExecuteAsync(filter);

                viewModel.Aliases.Should().HaveCount(count);
            },
            sqlBuilder,
            visitors
        );
    }
    
    [Theory]
    [InlineData("A", 1)]
    [InlineData("B", 1)]
    [InlineData("C", 1)]
    [InlineData("E", 0)]
    public async Task FilterRarelyUsedAliases(string filter, int count)
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.AskUserYesNoAsync(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.LowUsageThreshold = 10
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("A");
                                                 alias.WithUsage(
                                                     DateTime.Now.AddMonths(-10),
                                                     DateTime.Now.AddMonths(-10)
                                                 );
                                             }
                                         )
                                         .AppendAlias(
                                             2,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("B");
                                                 alias.WithUsage(
                                                     DateTime.Now.AddMonths(-10),
                                                     DateTime.Now.AddMonths(-10)
                                                 );
                                             }
                                         )
                                         .AppendAlias(
                                             3,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("C");
                                                 alias.WithUsage(
                                                     DateTime.Now.AddMonths(-10),
                                                     DateTime.Now.AddMonths(-10)
                                                 );
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.SetLowUsageThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowRarelyUsedAliasesCommand.ExecuteAsync(null);
                await viewModel.FilterAliasCommand.ExecuteAsync(filter);

                viewModel.Aliases.Should().HaveCount(count);
            },
            sqlBuilder,
            visitors
        );
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
    public async Task NotDeletePermanentlyWithoutValidation()
    {
        var visitors = new ServiceVisitors
        {
            VisitUserInteractionService = (_, i) =>
            {
                i.AskUserYesNoAsync(Arg.Any<object>())
                 .Returns(false);
                return i;
            }
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
            1,
            "name",
            props: new() { DeletedAt = DateTime.Now },
            cfg: a =>
            {
                a.WithSynonyms("a1", "a2");
                a.WithArguments(new() { ["1"] = "un", ["2"] = "deux", ["3"] = "trois" });
                a.WithUsage(
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025"),
                    DateTime.Parse("01/01/2025")
                );
            }
        );
        await TestViewModelAsync(
            async (viewModel, db) =>
            {
                // arrange
                await viewModel.ShowRestoreAliasesCommand.ExecuteAsync(null);

                // act
                viewModel.Aliases.ElementAt(0).IsSelected = true;
                await viewModel.DeletePermanentlyCommand.ExecuteAsync(null);

                // assert
                db.WithConnection(
                    c =>
                    {
                        using (new AssertionScope())
                        {
                            c.ExecuteScalar<long>("select count(*) from alias_usage where id_alias = 1")
                             .Should()
                             .BeGreaterThan(0, "usage should be cleared");

                            c.ExecuteScalar<long>("select count(*) from alias_name where id_alias = 1")
                             .Should()
                             .BeGreaterThan(0, "names should be cleared");

                            c.ExecuteScalar<long>("select count(*) from alias_argument where id_alias = 1")
                             .Should()
                             .BeGreaterThan(0, "arguments should be cleared");

                            c.ExecuteScalar<long>("select count(*) from alias where id = 1")
                             .Should()
                             .BeGreaterThan(0, "alias should be cleared");
                        }
                    }
                );
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
                                             new(DeletedAt: DateTime.Now),
                                             alias => alias.WithSynonyms("a1", "a2")
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
    public async Task ShowRarelyUsedAliases()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.AskUserYesNoAsync(Arg.Any<object>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.LowUsageThreshold = 3
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(1, cfg: alias =>
                                         {
                                             alias.WithSynonyms("A");
                                             alias.WithUsage(
                                                 DateTime.Now.AddMonths(-1), // Recent usage
                                                 DateTime.Now.AddMonths(-100)
                                             );
                                         })
                                         .AppendAlias(2, cfg: alias =>
                                         {
                                             alias.WithSynonyms("B");
                                             alias.WithUsage(
                                                 DateTime.Now.AddMonths(-100),
                                                 DateTime.Now.AddMonths(-110),
                                                 DateTime.Now.AddMonths(-120)
                                             );
                                         })
                                         .AppendAlias(
                                             3,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("C");
                                                 alias.WithUsage(
                                                     DateTime.Now,
                                                     DateTime.Now.AddMonths(-1), // Recent usage
                                                     DateTime.Now.AddMonths(-120),
                                                     DateTime.Now.AddMonths(-121),
                                                     DateTime.Now.AddMonths(-122),
                                                     DateTime.Now.AddMonths(-123),
                                                     DateTime.Now.AddMonths(-124)
                                                 );
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.SetInactivityThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowRarelyUsedAliasesCommand.ExecuteAsync(null);

                viewModel.Aliases.Should().HaveCount(1);
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
    public async Task ShowInactiveAliases()
    {
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                i.AskUserYesNoAsync(
                     Arg.Any<object>(),
                     Arg.Any<string>(),
                     Arg.Any<string>(),
                     Arg.Any<string>()
                 )
                 .Returns(true);
                return i;
            },
            VisitSettings = settings => settings.Application.Reconciliation.InactivityThreshold = 1
        };
        var sqlBuilder = new SqlBuilder().AppendAlias(
                                             1,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("A");
                                                 alias.WithUsage(
                                                     DateTime.Now.AddMonths(-1), // Recent usage
                                                     DateTime.Now.AddMonths(-100)
                                                 );
                                             }
                                         )
                                         .AppendAlias(
                                             2,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("B");
                                                 alias.WithUsage(
                                                     DateTime.Now.AddMonths(-100),
                                                     DateTime.Now.AddMonths(-110),
                                                     DateTime.Now.AddMonths(-120)
                                                 );
                                             }
                                         )
                                         .AppendAlias(
                                             3,
                                             cfg: alias =>
                                             {
                                                 alias.WithSynonyms("C");
                                                 alias.WithUsage(
                                                     DateTime.Now,
                                                     DateTime.Now.AddMonths(-1), // Recent usage
                                                     DateTime.Now.AddMonths(-120),
                                                     DateTime.Now.AddMonths(-121),
                                                     DateTime.Now.AddMonths(-122),
                                                     DateTime.Now.AddMonths(-123),
                                                     DateTime.Now.AddMonths(-124)
                                                 );
                                             }
                                         );
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.SetInactivityThresholdCommand.ExecuteAsync(null);
                await viewModel.ShowInactiveAliasesCommand.ExecuteAsync(null);

                viewModel.Aliases.Should().HaveCount(1);
            },
            sqlBuilder,
            visitors
        );
    }

    [Fact]
    public async Task ShowNeverUsedAliases()
    {
        var sqlBuilder = new SqlBuilder().AppendAlias(1, cfg: alias => alias.WithSynonyms("A"))
                                         .AppendAlias(2, cfg: alias => alias.WithSynonyms("B"))
                                         .AppendAlias(3, cfg: alias => alias.WithSynonyms("C"));
        await TestViewModelAsync(
            async (viewModel, _) =>
            {
                await viewModel.ShowUnusedAliasesCommand.ExecuteAsync(null);

                viewModel.Aliases.Should().HaveCount(3);
            },
            sqlBuilder
        );
    }

    [Fact]
    public async Task ShowRestoreAlias()
    {
        await TestViewModelAsync(
            async (viewModel, _) =>  await viewModel.ShowRestoreAliasesCommand.ExecuteAsync(null),
            SqlBuilder.Empty
        );
    }

    #endregion
}