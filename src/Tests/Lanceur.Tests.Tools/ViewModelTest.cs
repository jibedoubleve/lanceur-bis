using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.Tooling;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tools;

public abstract class ViewModelTest<TViewModel> : TestBase
    where TViewModel : class
{
    #region Constructors

    protected ViewModelTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected abstract IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors? visitors);

    protected async Task TestViewModel(Func<TViewModel, IDbConnectionManager, Task> scope, SqlBuilder? sqlBuilder = null, ServiceVisitors? visitors = null)
    {
        var connectionString = visitors?.OverridenConnectionString ??  ConnectionStringFactory.InMemory;
        using var db = GetDatabase(sqlBuilder ?? SqlBuilder.Empty, connectionString.ToString());

        var serviceCollection = new ServiceCollection().AddView<TViewModel>()
                                                       .AddLogging(builder => builder.AddXUnit(OutputHelper))
                                                       .AddDatabase(db);

        var serviceProvider = ConfigureServices(serviceCollection, visitors).BuildServiceProvider();
        var viewModel = serviceProvider.GetService<TViewModel>() !;
        await scope(viewModel, db);
    }

    #endregion
}