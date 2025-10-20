using System.Reflection;
using System.Web.Bookmarks;
using Shouldly;
using Lanceur.Core;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.ReservedAliases;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Stores;

public class ReservedKeywordsStoreShould
{
    #region Methods

    private static ReservedAliasStore GetStore(IAliasRepository aliasRepository, Type type = null)
    {
        type ??= typeof(NotExecutableTestAlias);

        var reservedAliasStoreLogger = Substitute.For<ILogger<ReservedAliasStore>>();
        var serviceProvider = new ServiceCollection()
                              .AddSingleton(new AssemblySource { ReservedKeywordSource = type.Assembly })
                              .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                              .AddSingleton(Substitute.For<IDatabaseConfigurationService>())
                              .AddSingleton(aliasRepository)
                              .AddSingleton<ILoggerFactory, LoggerFactory>()
                              .AddSingleton(reservedAliasStoreLogger)
                              .AddMockSingleton<IBookmarkRepositoryFactory>()
                              .AddMockSingleton<IConfigurationFacade>((_, i) =>
                                  {
                                      i.Application.Returns(new DatabaseConfiguration());
                                      return i;
                                  }
                              )
                              .BuildServiceProvider();

        var store = new ReservedAliasStore(serviceProvider);
        return store;
    }

    [Theory]
    [InlineData("add")]
    [InlineData("quit")]
    [InlineData("setup")]
    [InlineData("version")]
    [InlineData("clrbm")]
    public void ReturnSpecifiedReservedAliasFromLanceur(string criterion)
    {
        var repository = Substitute.For<IAliasRepository>();
        var store = GetStore(repository, typeof(MainView));
        var query = new Cmdline(criterion);

        store.Search(query).Count().ShouldBe(1);
    }

    [Fact]
    public void UpdateCounterOnSearch()
    {
        const int count = 100;
        const int id = 12;

        var aliasRepository = Substitute.For<IAliasRepository>();
        aliasRepository.GetHiddenCounters()
                       .Returns(new Dictionary<string, (long, int)> { { Names.Name1, (id, count) } });


        var sp = new ServiceCollection()
                 .AddSingleton(
                     new AssemblySource { ReservedKeywordSource = Assembly.GetAssembly(typeof(ExecutableTestAlias)) }
                 )
                 .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                 .AddSingleton(aliasRepository)
                 .AddSingleton(Substitute.For<ILogger<ReservedAliasStore>>())
                 .BuildServiceProvider();

        var store = new ReservedAliasStore(sp);

        var result = store.Search(Cmdline.Empty)
                          .ToArray();

        result.Length.ShouldBe(1);
        var current = result.First();
        current.Count.ShouldBe(count);
        current.Id.ShouldBe(id);
    }

    #endregion
}