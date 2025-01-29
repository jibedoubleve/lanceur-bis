using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Stores;
using NSubstitute;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Repositories.Config;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.BusinessLogic;

public class ReservedKeywordsStoreShould
{
    #region Methods

    private static ReservedAliasStore GetStore(IAliasRepository aliasRepository, Type type = null)
    {
        type ??= typeof(NotExecutableTestAlias);
        
        var reservedAliasStoreLogger = Substitute.For<ILogger<ReservedAliasStore>>();
        var serviceProvider = new ServiceCollection().AddSingleton(new AssemblySource { ReservedKeywordSource = type.Assembly })
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
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
    public void UpdateCounterOnSearch()
    {
        const int count = 100;
        const int id = 12;

        var aliasRepository = Substitute.For<IAliasRepository>();
        aliasRepository.GetHiddenCounters().Returns(new Dictionary<string, (long, int)> { { Names.Name1, (id, count) } });


        var sp = new ServiceCollection().AddSingleton(new AssemblySource { ReservedKeywordSource = Assembly.GetExecutingAssembly() })
                                        .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                        .AddSingleton(aliasRepository)
                                        .AddSingleton(Substitute.For<ILogger<ReservedAliasStore>>())
                                        .BuildServiceProvider();

        var store = new ReservedAliasStore(sp);

        var result = store.Search(Cmdline.Empty)
                          .ToArray();

        result.Should().HaveCount(1);
        var current = result.First();
        current.Count.Should().Be(count);
        current.Id.Should().Be(id);
    }
    #endregion
}