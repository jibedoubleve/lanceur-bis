using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tools;

public abstract class ViewModelTester<TViewModel> : TestBase
    where TViewModel : class
{
    #region Constructors

    protected ViewModelTester(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected abstract IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors? visitors
    );

    protected void TestViewModel(
        Action<TViewModel, IDbConnectionManager> scope,
        ISqlGenerator? sqlBuilder = null,
        ServiceVisitors? visitors = null
    )
    {
        var connectionString = visitors?.OverridenConnectionString ??  ConnectionStringFactory.InMemory;
        using var connectionManager = GetConnectionManager(sqlBuilder ?? Sql.Empty, connectionString.ToString());

        var serviceCollection = new ServiceCollection().AddView<TViewModel>()
                                                       .AddLogging(builder => builder.AddXUnit(OutputHelper))
                                                       .AddSingleton<IEnigma, Enigma>()
                                                       .AddDatabase(connectionManager);

        var serviceProvider = ConfigureServices(serviceCollection, visitors).BuildServiceProvider();
        var viewModel = serviceProvider.GetService<TViewModel>() !;
        scope(viewModel, connectionManager);
    }

    protected async Task TestViewModelAsync(
        Func<TViewModel, IDbConnectionManager, Task> scope,
        ISqlGenerator? sqlBuilder = null,
        ServiceVisitors? visitors = null
    )
    {
        var connectionString = visitors?.OverridenConnectionString ??  ConnectionStringFactory.InMemory;
        using var connectionManager = GetConnectionManager(sqlBuilder ?? Sql.Empty, connectionString.ToString());

        var serviceCollection = new ServiceCollection().AddView<TViewModel>()
                                                       .AddLogging(builder =>
                                                           {
                                                               builder.AddXUnit(OutputHelper);
                                                               builder.SetMinimumLevel(LogLevel.Trace);
                                                           }
                                                       )
                                                       .AddDatabase(connectionManager);

        var serviceProvider = ConfigureServices(serviceCollection, visitors).BuildServiceProvider();
        var viewModel = serviceProvider.GetService<TViewModel>() !;
        await scope(viewModel, connectionManager);
    }

    #endregion
}