using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class DataReconciliationViewModelShould : ViewModelTest<DataReconciliationViewModel>
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