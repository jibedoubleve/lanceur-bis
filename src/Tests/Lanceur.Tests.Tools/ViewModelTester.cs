using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.WPF.ReservedAliases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        ServiceHooks? hooks,
        out TViewModel viewModel
    )
    {
        DbSingleConnectionManager? connectionManager = null;
        try
        {
            var connectionString = hooks?.OverridenConnectionString ?? ConnectionStringFactory.InMemory;
            connectionManager = GetConnectionManager(sqlBuilder ?? Sql.Empty, connectionString.ToString());

            var serviceCollection = new ServiceCollection().AddSingleton<TViewModel>()
                                                           .AddSingleton<IEnigma, Enigma>()
                                                           .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose))
                                                           .AddMockSingleton<IUserGlobalNotificationService>()
                                                           .AddMockSingleton<IStoreShortcutService>()
                                                           .AddMockConfigurationSections(
                                                               hooks?.InterceptApplicationSettingsProvider)
                                                           .AddLogging(builder =>
                                                               builder.AddXUnit(OutputHelper)
                                                                      .SetMinimumLevel(LogLevel.Trace)
                                                           )
                                                           .AddReservedAliases(typeof(AddAlias))
                                                           .AddDatabase(connectionManager);

            hooks?.OverridenServices?.Invoke(serviceCollection);

            var serviceProvider = ConfigureServices(serviceCollection, hooks).BuildServiceProvider();
            viewModel = serviceProvider.GetService<TViewModel>()!;
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
        ServiceHooks? hooks
    );

    protected void TestViewModel(
        Action<TViewModel, IDbConnectionManager> scope,
        ISqlBuilder? sqlBuilder = null,
        ServiceHooks? Hooks = null
    )
    {
        using var connectionManager = ConfigureServices(sqlBuilder, Hooks, out var viewModel);
        scope(viewModel, connectionManager);
    }

    protected async Task TestViewModelAsync(
        Func<TViewModel, IDbConnectionManager, Task> scope,
        ISqlBuilder? sqlBuilder = null,
        ServiceHooks? hooks = null
    )
    {
        using var connectionManager = ConfigureServices(sqlBuilder, hooks, out var viewModel);
        await scope(viewModel, connectionManager);
    }

    #endregion
}