using System.Data;
using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Infra.Extensions;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.WPF.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Ui.IoC;

public sealed class CompositionRootTest : TestBase
{
    #region Constructors

    public CompositionRootTest(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    /// <summary>
    ///     Smoke test for the composition root. Verifies that all type-based dependencies
    ///     are satisfied (<see cref="ServiceProviderOptions.ValidateOnBuild" />),
    ///     then explicitly resolves services registered via factory lambda
    ///     that ValidateOnBuild cannot validate automatically.
    /// </summary>
    [Fact]
    public void When_composition_root_is_built_Then_all_services_can_be_resolved()
    {
        // arrange
        var services = new ServiceCollection()
                       .AddLoggingForTests(OutputHelper)
                       .AddInfraServices()
                       .AddDatabaseServices()
                       .AddWin32Services()
                       .AddUiCoreServices()
                       .AddWpfServices();

        // act - ValidateOnBuild checks the full type-based dependency graph
        var provider = Should.NotThrow(() =>
            services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true })
        );

        // assert - explicitly resolve services registered via factory lambda, which ValidateOnBuild cannot validate
        provider.ShouldSatisfyAllConditions(
            p => Should.NotThrow(p.GetRequiredService<IDbConnection>),
            p => Should.NotThrow(p.GetRequiredService<IDatabaseUpdater>),
            p => Should.NotThrow(() => p.GetServices<ISettingsProvider>().ShouldNotBeEmpty())
        );
    }

    #endregion
}
