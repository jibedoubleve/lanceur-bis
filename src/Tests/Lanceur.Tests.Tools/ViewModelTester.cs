using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.WPF.ReservedAliases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Lanceur.Tests.Tools;

public abstract class ViewModelTester<TViewModel> : TestBase
    where TViewModel : class
{
    #region Constructors

    protected ViewModelTester(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private DbSingleConnectionManager ConfigureServices(
        ISqlBuilder? sqlBuilder,
        ServiceVisitors? visitors,
        out TViewModel viewModel
    )
    {
        DbSingleConnectionManager? connectionManager = null;
        try
        {
            var connectionString = visitors?.OverridenConnectionString ??  ConnectionStringFactory.InMemory;
            connectionManager = GetConnectionManager(sqlBuilder ?? Sql.Empty, connectionString.ToString());

            var serviceCollection = new ServiceCollection().AddConfigurationSections()
                                                           .AddSingleton<TViewModel>()
                                                           .AddLogging(builder =>
                                                               builder.AddXUnit(OutputHelper)
                                                                      .SetMinimumLevel(LogLevel.Trace))
                                                           .AddSingleton<IEnigma, Enigma>()
                                                           .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose))
                                                           .AddReservedAliasesServices(typeof(AddAlias))
                                                           .AddDatabase(connectionManager);

            var serviceProvider = ConfigureServices(serviceCollection, visitors).BuildServiceProvider();
            viewModel = serviceProvider.GetService<TViewModel>() !;
            return connectionManager;
        }
        catch
        {
            connectionManager?.Dispose();
            throw;
        }
    }

    protected abstract IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors? visitors
    );

    protected void TestViewModel(
        Action<TViewModel, IDbConnectionManager> scope,
        ISqlBuilder? sqlBuilder = null,
        ServiceVisitors? visitors = null
    )
    {
        using var connectionManager = ConfigureServices(sqlBuilder, visitors, out var viewModel);
        scope(viewModel, connectionManager);
    }

    protected async Task TestViewModelAsync(
        Func<TViewModel, IDbConnectionManager, Task> scope,
        ISqlBuilder? sqlBuilder = null,
        ServiceVisitors? visitors = null
    )
    {
        using var connectionManager = ConfigureServices(sqlBuilder, visitors, out var viewModel);
        await scope(viewModel, connectionManager);
    }

    #endregion
}