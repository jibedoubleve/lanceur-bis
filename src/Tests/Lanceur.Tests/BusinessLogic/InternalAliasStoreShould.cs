using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Stores;
using NSubstitute;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Repositories.Config;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.BusinessLogic;

public class InternalAliasStoreShould
{
    #region Methods

    private static ReservedAliasStore GetStore(IAliasRepository aliasRepository, Type type = null)
    {
        type ??= typeof(NotExecutableTestAlias);
        
        var reservedAliasStoreLogger = Substitute.For<ILogger<ReservedAliasStore>>();
        var serviceProvider = new ServiceCollection().AddSingleton(new AssemblySource { ReservedKeywordSource = type.Assembly })
                                                     .AddSingleton(Substitute.For<IDatabaseConfigurationService>())
                                                     .AddSingleton(aliasRepository)
                                                     .AddSingleton<ILoggerFactory, LoggerFactory>()
                                                     .AddSingleton(reservedAliasStoreLogger)
                                                     .BuildServiceProvider();

        var store = new ReservedAliasStore(serviceProvider);
        return store;
    }

    [Theory]
    [InlineData("add")]
    [InlineData("quit")]
    [InlineData("setup")]
    [InlineData("version")]
    public void ReturnSpecifiedReservedAliasFromLanceur(string criterion)
    {
        var repository = Substitute.For<IAliasRepository>();
        var store = GetStore(repository, typeof(MainView));
        var query = new Cmdline(criterion);

        store.Search(query)
             .Should()
             .HaveCount(1);
    }

    [Fact]
    public void ReturnSpecifiedReservedKeyword()
    {
        var serviceProvider  = new ServiceCollection().AddMockSingleton<IAliasRepository>((_, repository) => repository)
                                                      .BuildServiceProvider();

        var store = GetStore(serviceProvider.GetService<IAliasRepository>());
        var query = new Cmdline("anothertest");

        store.Search(query)
             .Should()
             .HaveCount(1);
    }

    #endregion
}